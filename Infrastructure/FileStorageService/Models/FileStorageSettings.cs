using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.FileStorageService.Models
{
    public class FileStorageSettings
    {
        public long MaxFileSize { get; set; }
        public string SubtitlesPath { get; set; }
    }
}