using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class FileScanner
    {
        public static List<string> GetFilesByExtensions(string rootPath, params string[] extensions)
        {
            if (!Directory.Exists(rootPath))
            {
                return new List<string>();
            }

            return Directory
                .EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }

}


