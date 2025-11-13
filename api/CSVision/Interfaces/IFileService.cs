namespace CSVision.Interfaces
{
    public interface IFileService
    {
        Task ValidateCsvFileAsync(IFormFile file);
        IFormFile RemoveIdColumnAsync(IFormFile file);
    }
}
