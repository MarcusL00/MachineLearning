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

        public abstract ModelResult TrainModel(IFormFile file);

        protected abstract IEstimator<ITransformer> BuildTrainer(MLContext mlContext);

        protected abstract IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext);

        protected abstract (
            Dictionary<string, double>,
            ConfusionMatrix? confusionMatrix
        ) EvaluateModel(MLContext mlContext, IDataView predictions);

        /// <summary>
        /// Template method for training a model with reusable steps.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private protected ModelResult TrainWithTemplate(IFormFile file)
        {
            // Initialize MLContext and load data
            var mlContext = Seed == -1 ? new MLContext() : new MLContext(Seed);
            var dataView = CreateDataViewFromCsvFile(file, mlContext, out string tempCleaned);

            // Split data into train and test sets (80/20)
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Build feature + target pipeline
            var baseEstimator = BuildFeaturePipeline(mlContext, dataView, Target)
                .Append(BuildLabelConversion(mlContext));

            // Fit base transformations
            var baseTransformer = baseEstimator.Fit(split.TrainSet);
            var transformedTrain = baseTransformer.Transform(split.TrainSet);

            // Train model
            var trainerEstimator = BuildTrainer(mlContext);

            ITransformer trainerTransformer = trainerEstimator.Fit(transformedTrain);

            // Evaluate model
            var predictions = trainerTransformer.Transform(transformedTrain);
            var (metrics, confusionMatrix) = EvaluateModel(mlContext, predictions);

            // Extract prediction values - handle Key vs numeric Label types
            double[] ActualValues;
            double[] PredictedValues;

            // Inspect schema to determine Label type
            var schema = predictions.Schema;
            bool isLabelBool = false;
            for (int i = 0; i < schema.Count; i++)
            {
                if (schema[i].Name == "Label")
                {
                    if (schema[i].Type == BooleanDataViewType.Instance)
                    {
                        isLabelBool = true;
                    }
                }
            }

            if (isLabelBool)
            {
                var binPreds = mlContext
                    .Data.CreateEnumerable<BinaryPredictionRow>(predictions, reuseRowObject: false)
                    .ToList();
                ActualValues = binPreds.Select(p => p.Label ? 1.0 : 0.0).ToArray();
                PredictedValues = binPreds.Select(p => (double)p.Score).ToArray();
            }
            else
            {
                // Regression/Binary: Label is numeric
                var numPreds = mlContext
                    .Data.CreateEnumerable<NumericPredictionRow>(predictions, reuseRowObject: false)
                    .ToList();
                ActualValues = numPreds.Select(p => (double)p.Label).ToArray();
                PredictedValues = numPreds.Select(p => (double)p.Score).ToArray();
            }

            // Compose final model
            var model = baseTransformer.Append(trainerTransformer);

            FileUtilities.DeleteTempFile(tempCleaned);

            return new ModelResult
            {
                ModelName = ModelName,
                Metrics = metrics,
                ConfusionMatrix = confusionMatrix,
                ActualValues = ActualValues,
                PredictionValues = PredictedValues,
            };
        }

        private protected IEstimator<ITransformer> BuildFeaturePipeline(
            MLContext mlContext,
            IDataView dataView,
            string targetColumn
        )
        {
            // Build transformations for features
            var transforms = new List<IEstimator<ITransformer>>();
            var featureColumns = new List<string>();

            // Map the provided feature names to the schema in the IDataView so
            // we know the runtime DataViewType for each column. If a feature
            // name is not found in the IDataView schema we mark its type as
            // null so it can be skipped later.
            IEnumerable<(string name, DataViewType? type)> columnsToProcess = Features
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

                    // Feature not present in schema
                    return (name: f, type: (DataViewType?)null);
                });

            // For each discovered feature, build an appropriate transform:
            // - If the column is text, featurize it into a numeric vector.
            // - Otherwise, convert the column to `float` (Single) so all
            //   numeric features share the same dtype for concatenation.
            // Each transform writes into a dedicated output column named
            // `{originalName}_num`, which is collected into `featureColumns`.
            foreach (var (name, type) in columnsToProcess)
            {
                var outputCol = name + "_num";

                if (type == null)
                    // Skip features that don't exist in the data schema
                    continue;

                if (type.RawType == typeof(string) || type is TextDataViewType)
                {
                    // Text columns: produce a numeric feature vector using
                    // ML.NET's text featurization (ngrams, TF/IDF, etc.).
                    transforms.Add(
                        mlContext.Transforms.Text.FeaturizeText(
                            outputColumnName: outputCol,
                            inputColumnName: name
                        )
                    );
                }
                else
                {
                    // Non-text columns: coerce to `Single` (float) so they
                    // can be concatenated into a single `Features` vector.
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

            // Concatenate all per-feature numeric outputs into the final
            // `Features` column required by ML.NET trainers.
            transforms.Add(mlContext.Transforms.Concatenate("Features", featureColumns.ToArray()));

            // Chain all transforms together into a single estimator and return it.
            return transforms.Aggregate((current, next) => current.Append(next));
        }

        private protected static IDataView CreateDataViewFromCsvFile(
            IFormFile file,
            MLContext mlContext,
            out string tempCsvFilePath
        )
        {
            // Create cleaned temporary CSV file and retrieve path
            tempCsvFilePath = FileUtilities.CreateTempFile(file);

            // Load data from the temp CSV file
            string[] lines = File.ReadAllLines(tempCsvFilePath);

            // Define columns based on header
            var headers = lines[0];
            var dividedHeaders = headers.Split(',');

            // Define TextLoader columns
            var columns = new List<TextLoader.Column>();
            for (int i = 0; i < dividedHeaders.Length; i++)
            {
                // Determine column name, defaulting unnamed to "Column{i}"
                string name = string.IsNullOrWhiteSpace(dividedHeaders[i])
                    ? $"Column{i}"
                    : dividedHeaders[i];

                // For simplicity, assume all columns are numeric (DataKind.Single)
                columns.Add(new TextLoader.Column(name, DataKind.Single, i));
            }

            // Create TextLoader with the defined columns
            // Used to create IDataView from the CSV file
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
                // Attempt to load as multi-file source in case of issues
                return textLoader.Load(new MultiFileSource(tempCsvFilePath));
            }
            catch
            {
                // Fallback to single file load
                return textLoader.Load(tempCsvFilePath);
            }
        }
    }
}
