using Microsoft.AspNetCore.Http;

namespace Infrastructure.Utilities
{
    public class ZipTools
    {
        public static async Task<bool> IsZipFile(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            byte[] buffer = new byte[4];
            await stream.ReadExactlyAsync(buffer.AsMemory(0, 4));
            return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
        }

        public static async Task<bool> IsZipFile(string path)
        {
            if (!File.Exists(path))
                return false;

            byte[] buffer = new byte[4];
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bytesRead = await stream.ReadAsync(buffer, 0, 4);
            if (bytesRead < 4)
                return false;

            return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
        }
    }

}