namespace CSVision.Utilities
{
    internal static class FileUtilities
    {
        internal static string CreateTempFile(IFormFile file)
        {
            var tempFileName = Path.GetTempFileName();
            using (
                var inputFileStream = new FileStream(
                    tempFileName,
                    FileMode.Create,
                    FileAccess.Write
                )
            )
            {
                file.CopyTo(inputFileStream);
            }

            return tempFileName;
        }
    }
}
