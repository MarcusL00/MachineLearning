using System.Text;
using CSVision.Interfaces;

namespace CSVision.Services
{
    public sealed class FileService : IFileService
    {
        public Task ValidateCsvFileAsync(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public IFormFile RemoveIdColumnAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            // Read header line
            var headerLine = reader.ReadLine();
            var headers = headerLine.Split(',');

            // Find indices of unnamed columns (empty or whitespace)
            var unnamedIndices = headers
                .Select((h, i) => new { h, i })
                .Where(x => string.IsNullOrWhiteSpace(x.h))
                .Select(x => x.i)
                .ToHashSet();

            // Build new header without unnamed columns
            var cleanedHeader = string.Join(
                ",",
                headers.Where((h, i) => !unnamedIndices.Contains(i))
            );

            var cleanedLines = new List<string> { cleanedHeader };

            // Process remaining rows
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var parts = line.Split(',');

                var filtered = parts.Where((col, idx) => !unnamedIndices.Contains(idx));
                cleanedLines.Add(string.Join(",", filtered));
            }

            // Convert cleaned CSV back into a stream
            var cleanedCsv = string.Join(Environment.NewLine, cleanedLines);
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(cleanedCsv));

            // Create a new IFormFile from the cleaned stream
            var cleanedFile = new FormFile(
                memoryStream,
                0,
                memoryStream.Length,
                file.Name,
                file.FileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = file.ContentType,
            };

            return cleanedFile;
        }
    }
}
