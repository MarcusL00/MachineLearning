namespace CSVision.Interfaces
{
    public interface IFileService
    {
        IFormFile CleanseCsvFile(IFormFile file);
        bool IsValidLength(IFormFile file, uint minimumLines);
    }
}
