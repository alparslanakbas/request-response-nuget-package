namespace RRM_File_Logger.Library.Models
{
    public class FileLoggingOptions
    {
        public string FileDirectory { get; set; }
        public string FileName { get; set; } = "log";
        public string Extension { get; set; } = ".txt";
        public bool ForceCreateDirectory { get; set; } = true;
        public bool UseJsonFormat { get; set; } = false;


        internal string GetFullFileName() => $"{FileName}.{Extension}";
        internal string GetFullFilePath() => Path.Combine(FileDirectory, GetFullFileName());
        internal void ValidatePath()
        {
            try
            {
                if (ForceCreateDirectory)
                    Directory.CreateDirectory(FileDirectory);

                else
                {
                    if (!Directory.Exists(FileDirectory))
                        throw new DirectoryNotFoundException($"Directory {FileDirectory} does not exist.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
        }
    }
}
