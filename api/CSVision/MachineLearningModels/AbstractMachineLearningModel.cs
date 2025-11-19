using CSVision.Models;
using CSVision.Utilities;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public abstract class AbstractMachineLearningModel
    {
        internal abstract string ModelName { get; }
        private protected string[] Features { get; }
        private protected string Target { get; }
        private protected int Seed { get; }

        internal AbstractMachineLearningModel(string[] features, string target, int seed)
        {
            Features = features;
            Target = target;
            Seed = seed;
        }

        public abstract byte[] GeneratePredictionGraph(double[] actualValues, double[] predictedValues);

        public abstract ModelResult TrainModel(IFormFile file);

        // Child classes must provide their trainer
        protected abstract IEstimator<ITransformer> BuildTrainer(MLContext mlContext);

        protected abstract IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext);

        // Child classes must provide their evaluator
        protected abstract Dictionary<string, double> EvaluateModel(
            MLContext mlContext,
            IDataView predictions
        );

        /// <summary>
        /// Generic training template: handles pipeline, train/test split, fitting, evaluation.
        /// </summary>
        private protected ModelResult TrainWithTemplate(IFormFile file)
        {
            var mlContext = Seed == -1 ? new MLContext() : new MLContext(Seed);
            var dataView = CreateDataViewFromCsvFile(file, out string tempCleaned);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Build feature + label pipeline
            var baseEstimator = BuildFeaturePipeline(mlContext, dataView, Target)
                .Append(BuildLabelConversion(mlContext));

            var baseTransformer = baseEstimator.Fit(split.TrainSet);
            var transformedTrain = baseTransformer.Transform(split.TrainSet);

            // Train
            var trainerEstimator = BuildTrainer(mlContext);
            ITransformer trainerTransformer;
            try
            {
                trainerTransformer = trainerEstimator.Fit(transformedTrain);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Trainer.Fit exception: " + ex.Message);
                try
                {
                    var preview = transformedTrain.Preview(maxRows: 10);
                    Console.WriteLine("--- Transformed train preview ---");
                    foreach (var col in preview.ColumnView)
                    {
                        Console.WriteLine(
                            $"Column: {col.Column.Name} (Type: {col.Column.Type}) Values: {string.Join(",", col.Values.Select(v => v?.ToString() ?? "<null>"))}"
                        );
                    }
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Failed to preview transformed train: " + ex2.Message);
                }

                throw;
            }

            // Evaluate
            var transformedTrainData = baseTransformer.Transform(split.TrainSet);
            var predictions = trainerTransformer.Transform(transformedTrainData);
            var metrics = EvaluateModel(mlContext, predictions);

            // Extract prediction values - handle Key vs numeric Label types
            double[] ActualValues;
            double[] PredictedValues;

            var schema = predictions.Schema;
            bool isLabelKey = false;
            bool isLabelBool = false;
            for (int i = 0; i < schema.Count; i++)
            {
                if (schema[i].Name == "Label")
                {
                    if (schema[i].Type is KeyDataViewType)
                        isLabelKey = true;
                    else if (schema[i].Type == BooleanDataViewType.Instance)
                        isLabelBool = true;
                }
            }

            if (isLabelKey)
            {
                // Multiclass: Label is Key type, use PredictedLabel for predicted class
                var keyPreds = mlContext.Data.CreateEnumerable<MulticlassPredictionRow>(predictions, reuseRowObject: false).ToList();
                ActualValues = keyPreds.Select(p => (double)p.Label).ToArray();
                PredictedValues = keyPreds.Select(p => (double)p.PredictedLabel).ToArray();
            }
            else if (isLabelBool)
            {
                var binPreds = mlContext.Data.CreateEnumerable<BinaryPredictionRow>(predictions, reuseRowObject: false).ToList();
                ActualValues = binPreds.Select(p => p.Label ? 1.0 : 0.0).ToArray();
                PredictedValues = binPreds.Select(p => (double)p.Score).ToArray();
            }
            else
            {
                // Regression/Binary: Label is numeric
                var numPreds = mlContext.Data.CreateEnumerable<NumericPredictionRow>(predictions, reuseRowObject: false).ToList();
                ActualValues = numPreds.Select(p => (double)p.Label).ToArray();
                PredictedValues = numPreds.Select(p => (double)p.Score).ToArray();
            }

            // Compose final model
            var model = baseTransformer.Append(trainerTransformer);

        

            FileUtilities.DeleteTempFile(tempCleaned);

            return new ModelResult
            {
                ModelName = ModelName,
                TrainedModel = model,
                Metrics = metrics,
                Actuals = ActualValues,
                Predictions = PredictedValues,
            };
        }

        private protected IEstimator<ITransformer> BuildFeaturePipeline(
            MLContext mlContext,
            IDataView dataView,
            string targetColumn
        )
        {
            var transforms = new List<IEstimator<ITransformer>>();
            var featureColumns = new List<string>();

            IEnumerable<(string name, DataViewType? type)> columnsToProcess;
            if (Features != null && Features.Length > 0)
            {
                columnsToProcess = Features
                    .Where(f => !string.Equals(f, targetColumn, StringComparison.OrdinalIgnoreCase))
                    .Select(f =>
                    {
                        int foundIdx = -1;
                        for (int i = 0; i < dataView.Schema.Count; i++)
                        {
                            if (dataView.Schema[i].Name == f)
                            {
                                foundIdx = i;
                                break;
                            }
                        }

                        if (foundIdx >= 0)
                            return (name: f, type: dataView.Schema[foundIdx].Type);

                        return (name: f, type: (DataViewType?)null);
                    });
            }
            else
            {
                columnsToProcess = dataView
                    .Schema.Where(c => c.Name != targetColumn)
                    .Select(c => (name: c.Name, type: (DataViewType?)c.Type));
            }

            foreach (var (name, type) in columnsToProcess)
            {
                var outputCol = name + "_num";

                if (type == null)
                    continue;

                if (type.RawType == typeof(string) || type is TextDataViewType)
                {
                    transforms.Add(
                        mlContext.Transforms.Text.FeaturizeText(
                            outputColumnName: outputCol,
                            inputColumnName: name
                        )
                    );
                }
                else
                {
                    transforms.Add(
                        mlContext.Transforms.Conversion.ConvertType(
                            outputColumnName: outputCol,
                            inputColumnName: name,
                            outputKind: DataKind.Single
                        )
                    );
                }

                featureColumns.Add(outputCol);
            }

            transforms.Add(mlContext.Transforms.Concatenate("Features", featureColumns.ToArray()));

            return transforms.Aggregate((current, next) => current.Append(next));
        }

        private protected IDataView CreateDataViewFromCsvFile(
            IFormFile file,
            out string tempCleaned
        )
        {
            var mlContext = new MLContext();

            tempCleaned = FileUtilities.CreateTempFile(file);
            string[] lines = File.ReadAllLines(tempCleaned);

            var cleanedHeader = lines[0];
            var cleanedHeaders = cleanedHeader.Split(',');
            var sampleRows = lines.Skip(1).Take(10).Select(l => l.Split(',')).ToList();

            var columns = new List<TextLoader.Column>();
            for (int i = 0; i < cleanedHeaders.Length; i++)
            {
                string name = string.IsNullOrWhiteSpace(cleanedHeaders[i])
                    ? $"Column{i}"
                    : cleanedHeaders[i];

                bool isNumeric = sampleRows
                    .Select(r => r.Length > i ? r[i] : null)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .All(v => float.TryParse(v, out _));

                columns.Add(
                    new TextLoader.Column(name, isNumeric ? DataKind.Single : DataKind.String, i)
                );
            }

            var textLoader = mlContext.Data.CreateTextLoader(
                new TextLoader.Options
                {
                    Separators = new[] { ',' },
                    HasHeader = true,
                    Columns = columns.ToArray(),
                }
            );

            try
            {
                return textLoader.Load(new MultiFileSource(tempCleaned));
            }
            catch
            {
                return textLoader.Load(tempCleaned);
            }
        }
    }
}
