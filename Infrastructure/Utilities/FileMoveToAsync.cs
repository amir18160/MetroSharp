using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class FileMoveToAsync
    {
        public static Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path must be provided.", nameof(sourcePath));
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path must be provided.", nameof(destinationPath));

            return Task.Run(() =>
            {
                // Ensure target directory exists
                var destDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                // If file already exists at destination, delete it
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);

                // Move the file
                File.Move(sourcePath, destinationPath);
            });
        }
    }
}