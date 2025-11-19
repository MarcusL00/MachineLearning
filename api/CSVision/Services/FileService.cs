using System.Text;
using CSVision.Interfaces;
using CSVision.Utilities;

namespace CSVision.Services
{
    public sealed class FileService : IFileService
    {
        public bool IsValidLength(IFormFile file, uint minimumLines)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            uint lineCount = 0;
            while (reader.ReadLine() is not null)
            {
                lineCount++;
                if (lineCount >= minimumLines)
                    return true;
            }
            return false;
        }

        public IFormFile CleanseCsvFileAsync(IFormFile file)
        {
            string tempInput = string.Empty;

            try
            {
                // Copy uploaded file to a temp path
                tempInput = FileUtilities.CreateTempFile(file);

                // Read and clean: remove unnamed header columns and their data
                string[] lines = File.ReadAllLines(tempInput);
                if (lines.Length == 0)
                    throw new InvalidOperationException("CSV is empty.");

                var headers = lines[0].Split(',');
                var cleanedLines = new List<string>(capacity: lines.Length)
                {
                    string.Join(",", headers.Where((h, i) => !string.IsNullOrWhiteSpace(h))),
                };
                for (int r = 1; r < lines.Length; r++)
                {
                    var row = lines[r] ?? string.Empty;
                    var parts = row.Split(',');
                    var filtered = parts.Where(
                        (col, idx) =>
                            idx < headers.Length && !string.IsNullOrWhiteSpace(headers[idx])
                    );
                    cleanedLines.Add(string.Join(",", filtered));
                }

                var cleanedFile = CreateIFormFileFromCleanedData(file, cleanedLines);
                return cleanedFile;
            }
            finally
            {
                FileUtilities.DeleteTempFile(tempInput);
            }
        }

        private static IFormFile CreateIFormFileFromCleanedData(
            IFormFile originalFile,
            List<string> cleanedLines
        )
        {
            var cleanedCsv = string.Join(Environment.NewLine, cleanedLines);
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(cleanedCsv));

            var cleanedFile = new FormFile(
                memoryStream,
                0,
                memoryStream.Length,
                originalFile.Name,
                originalFile.FileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = originalFile.ContentType,
            };

            return cleanedFile;
        }
    }
}
