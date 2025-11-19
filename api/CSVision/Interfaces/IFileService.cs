namespace CSVision.Interfaces
{
    public interface IFileService
    {
        IFormFile CleanseCsvFileAsync(IFormFile file);
        bool IsValidLength(IFormFile file, uint minimumLines);
    }
}
