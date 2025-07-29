using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = Roles.Owner)]
    public class LogsController : BaseApiController
    {
        private readonly string _logsDirectory = "logs/server";

        [HttpGet("get-logs")]
        public IActionResult GetLogFiles()
        {
            if (!Directory.Exists(_logsDirectory))
            {
                return Ok(new List<string>());
            }

            var logFiles = Directory.GetFiles(_logsDirectory)
                .Select(Path.GetFileName)
                .OrderByDescending(f => f)
                .ToList();

            return Ok(logFiles);
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetLogFile(string fileName)
        {
            var filePath = Path.Combine(_logsDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Log file not found.");
            }

            try
            {
                using var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );

                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();

                var lines = content
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                // Combine lines as a JSON array
                var jsonArray = "[" + string.Join(",", lines) + "]";

                return Content(jsonArray, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to read log file: {ex.Message}");
            }
        }
    }
}
