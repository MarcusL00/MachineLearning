namespace CSVision.Interfaces
{
    public interface IFileService
    {
        IFormFile CleanseCsvFileAsync(IFormFile file);
    }
}
