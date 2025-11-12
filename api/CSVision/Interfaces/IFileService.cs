namespace CSVision.Interfaces
{
    public interface IFileService
    {
        Task ValidateCsvFileAsync(IFormFile file);
    }
}
