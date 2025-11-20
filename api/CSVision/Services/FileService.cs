using System.Text;
using CSVision.Interfaces;
using CSVision.Utilities;

namespace CSVision.Services
{
    public sealed class FileService : IFileService
    {
        /// <summary>
        /// Checks if the provided CSV file has at least the specified minimum number of lines.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="minimumLines"></param>
        /// <returns></returns>
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

        public IFormFile CleanseCsvFile(IFormFile file)
        {
            string tempCsvDataFilePath = string.Empty;

            try
            {
                // Copy uploaded file to a temp path and get the file path
                tempCsvDataFilePath = FileUtilities.CreateTempFile(file);

                // Read and clean: remove unnamed header columns and their data
                string[] lines = File.ReadAllLines(tempCsvDataFilePath);

                // Ensure there is at least one line (header)
                if (lines.Length == 0)
                    throw new InvalidOperationException("CSV is empty.");

                // Identify named headers and filter out unnamed columns
                var headers = lines[0].Split(',');
                var cleanedLines = new List<string>(capacity: lines.Length)
                {
                    string.Join(",", headers.Where((h, i) => !string.IsNullOrWhiteSpace(h))),
                };

                // Filter out unnamed columns from each data row
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

                // Create a new IFormFile from the cleaned data
                var cleanedFile = CreateIFormFileFromCleanedData(file, cleanedLines);
                return cleanedFile;
            }
            finally
            {
                // Delete the temporary data file
                FileUtilities.DeleteTempFile(tempCsvDataFilePath);
            }
        }

        private static IFormFile CreateIFormFileFromCleanedData(
            IFormFile originalFile,
            List<string> cleanedLines
        )
        {
            // Convert cleaned lines back to a single CSV string
            var cleanedCsv = string.Join(Environment.NewLine, cleanedLines);
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(cleanedCsv));

            // Create a new IFormFile with the cleaned data
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
