using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public abstract class AbstractMachineLearningModel
    {
        internal abstract string ModelName { get; }
        private protected string[] features { get; }

        // TODO: Split this up, so it looks cleaner
        private protected IDataView HandleCSV(IFormFile file)
        {
            var mlContext = new MLContext();

            // 1) Copy uploaded file to a temp path
            var tempInput = Path.GetTempFileName();
            using (var inputFs = new FileStream(tempInput, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(inputFs);
            }

            // 2) Read and clean: remove unnamed header columns and their data
            string[] lines = File.ReadAllLines(tempInput);
            if (lines.Length == 0)
                throw new InvalidOperationException("CSV is empty.");

            // Parse header
            var headers = lines[0].Split(',');
            var unnamedIndices = headers
                .Select((h, i) => new { h, i })
                .Where(x => string.IsNullOrWhiteSpace(x.h))
                .Select(x => x.i)
                .ToHashSet();

            // Build cleaned header excluding unnamed columns
            var cleanedHeader = string.Join(
                ",",
                headers.Where((h, i) => !unnamedIndices.Contains(i))
            );

            // Filter each data line by excluding unnamed indices
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
            var columns = cleanedHeaders
                .Select(
                    (h, i) =>
                        new TextLoader.Column(
                            string.IsNullOrWhiteSpace(h) ? $"Column{i}" : h,
                            DataKind.String,
                            i
                        )
                )
                .ToArray();

            var textLoader = mlContext.Data.CreateTextLoader(
                new TextLoader.Options
                {
                    Separators = new[] { ',' },
                    HasHeader = true,
                    Columns = columns,
                }
            );

            // 5) Load IDataView from the cleaned file
            // Works across ML.NET versions: use MultiFileSource or pass the path overload
            // If your version only accepts IMultiStreamSource, use: new MultiFileSource(tempCleaned)
            IDataView dataView;
            try
            {
                // Try the IMultiStreamSource path (preferred in newer versions)
                dataView = textLoader.Load(new MultiFileSource(tempCleaned));
            }
            catch
            {
                // Fallback: some versions allow passing the file path directly
                dataView = textLoader.Load(tempCleaned);
            }

            // 6) Optional: cleanup tempInput; keep tempCleaned if you need it later
            try
            {
                File.Delete(tempInput);
            }
            catch
            { /* ignore */
            }

            return dataView;
        }
    }
}
