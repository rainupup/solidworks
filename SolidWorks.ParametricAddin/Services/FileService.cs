using System;
using System.IO;

namespace SolidWorks.ParametricAddin.Services
{
    /// <summary>
    /// General file system operations helper.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Checks if the output directory is valid and writable.
        /// </summary>
        public bool IsOutputDirectoryValid(string directoryPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(directoryPath))
            {
                errorMessage = "输出目录未设置。";
                return false;
            }

            try
            {
                if (Directory.Exists(directoryPath))
                {
                    // Try creating a temp file to test write permission
                    string testFile = Path.Combine(directoryPath, ".write_test");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    return true;
                }

                // Directory does not exist — try to create it
                Directory.CreateDirectory(directoryPath);
                Directory.Delete(directoryPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "没有权限写入输出目录。";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"输出目录无效: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets available disk space in bytes for a given path.
        /// </summary>
        public long GetAvailableDiskSpace(string path)
        {
            try
            {
                string root = Path.GetPathRoot(path) ?? "C:\\";
                var driveInfo = new DriveInfo(root);
                return driveInfo.AvailableFreeSpace;
            }
            catch
            {
                return long.MaxValue; // Assume sufficient space
            }
        }

        /// <summary>
        /// Copies a file with a new name in the same or different directory.
        /// </summary>
        public bool CopyFile(string sourcePath, string destPath, bool overwrite = false)
        {
            try
            {
                string destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(sourcePath, destPath, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sanitizes a string for use as a filename.
        /// </summary>
        public string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
