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

        private protected IEstimator<ITransformer> BuildFeaturePipeline(
            MLContext mlContext,
            IDataView dataView,
            string targetColumn
        )
        {
            var transforms = new List<IEstimator<ITransformer>>();
            var featureColumns = new List<string>();

            // If the user provided an explicit Features list, use it. Otherwise fall back
            // to scanning the dataView schema and using every column except the target.
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

                // If we couldn't find the column in the schema, skip it
                if (type == null)
                    continue;

                // If the source column is textual, featurize it (handles categorical/text data).
                // Otherwise, try to convert to single (numeric).
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

            // Concatenate all transformed features
            transforms.Add(mlContext.Transforms.Concatenate("Features", featureColumns.ToArray()));

            // Return the chain of transforms (no trainer yet)
            return transforms.Aggregate((current, next) => current.Append(next));
        }

        // Build an IDataView from an already-cleaned IFormFile. This allows file handling
        // (cleaning, temp paths) to live in FileService while ML code converts the cleaned
        // file into an IDataView. FileService should return a cleaned IFormFile which can
        // be passed here.
        private protected IDataView CreateDataViewFromCsvFile(IFormFile file)
        {
            var mlContext = new MLContext();

            string tempCleaned = string.Empty;

            try
            {
                // Persist IFormFile to temp path so TextLoader can read it
                tempCleaned = FileUtilities.CreateTempFile(file);

                // Read header and sample rows
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
                        new TextLoader.Column(
                            name,
                            isNumeric ? DataKind.Single : DataKind.String,
                            i
                        )
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
            finally
            {
                FileUtilities.DeleteTempFile(tempCleaned);
            }
        }
    }
}
