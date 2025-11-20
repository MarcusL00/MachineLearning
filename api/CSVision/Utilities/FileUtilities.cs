namespace CSVision.Utilities
{
    internal static class FileUtilities
    {
        /// <summary>
        /// Creates a temporary file from the provided IFormFile and returns the file path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes the temporary file at the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        internal static void DeleteTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
