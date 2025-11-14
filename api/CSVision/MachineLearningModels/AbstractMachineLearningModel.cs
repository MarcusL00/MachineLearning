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
        private protected string[] Targets { get; }

        internal AbstractMachineLearningModel(string[] features, string[] targets)
        {
            Features = features;
            Targets = targets;
        }

        public abstract ModelResult TrainModel(IFormFile file);

        // TODO: Split this up, so it looks cleaner
        private protected IDataView HandleCSV(IFormFile file)
        {
            var mlContext = new MLContext();

            // 1) Copy uploaded file to a temp path
            var tempInput = FileUtilities.CreateTempFile(file);

            // 2) Read and clean: remove unnamed header columns and their data
            string[] lines = File.ReadAllLines(tempInput);
            if (lines.Length == 0)
                throw new InvalidOperationException("CSV is empty.");

            var headers = lines[0].Split(',');
            var unnamedIndices = headers
                .Select((h, i) => new { h, i })
                .Where(x => string.IsNullOrWhiteSpace(x.h))
                .Select(x => x.i)
                .ToHashSet();

            var cleanedHeader = string.Join(
                ",",
                headers.Where((h, i) => !unnamedIndices.Contains(i))
            );

            var cleanedLines = new List<string>(capacity: lines.Length) { cleanedHeader };
            for (int r = 1; r < lines.Length; r++)
            {
                var parts = lines[r].Split(',');
                var filtered = parts.Where((col, idx) => !unnamedIndices.Contains(idx));
                cleanedLines.Add(string.Join(",", filtered));
            }

            // 3) Persist cleaned CSV to a new temp file
            var tempCleaned = Path.GetTempFileName();
            File.WriteAllLines(tempCleaned, cleanedLines);

            // 4) Build dynamic TextLoader schema from cleaned header
            var cleanedHeaders = cleanedHeader.Split(',');
            var sampleRows = cleanedLines.Skip(1).Take(10).Select(l => l.Split(',')).ToList();

            var columns = new List<TextLoader.Column>();
            for (int i = 0; i < cleanedHeaders.Length; i++)
            {
                string name = string.IsNullOrWhiteSpace(cleanedHeaders[i])
                    ? $"Column{i}"
                    : cleanedHeaders[i];

                // Detect numeric vs text by sampling values
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

            // 5) Load IDataView from the cleaned file
            IDataView dataView;
            try
            {
                dataView = textLoader.Load(new MultiFileSource(tempCleaned));
            }
            catch
            {
                dataView = textLoader.Load(tempCleaned);
            }

            // 6) Cleanup tempInput
            try
            {
                File.Delete(tempInput);
            }
            catch
            { /* ignore */
            }

            return dataView;
        }

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
                columnsToProcess = dataView.Schema
                    .Where(c => c.Name != targetColumn)
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
    }
}
