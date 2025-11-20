namespace CSVision.Utilities
{
    internal static class FileUtilities
    {
        internal static string CreateTempFile(IFormFile file)
        {
            string tempFileName = Path.GetTempFileName();
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

        internal static void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
