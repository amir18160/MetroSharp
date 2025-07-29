using System.Diagnostics;
using System.Runtime.InteropServices;
using Application.Dashboard.Queries.GetSystemInfo;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SystemInfo
{
    public class SystemInfoService : ISystemInfoService
    {
        private readonly ILogger<SystemInfoService> _logger;

        public SystemInfoService(ILogger<SystemInfoService> logger)
        {
            _logger = logger;
        }

        public async Task<SystemInfoDto> GetSystemInfoAsync()
        {
            var dto = new SystemInfoDto
            {
                OsDescription = RuntimeInformation.OSDescription,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                CpuCoreCount = Environment.ProcessorCount,
                Drives = GetDriveInfo()
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dto.CpuName = GetCpuNameWindows();
                var ramInfo = GetRamInfoWindows();
                dto.TotalRamMb = ramInfo.Total;
                dto.UsedRamMb = ramInfo.Used;
                dto.RamUsagePercentage = ramInfo.Percentage;
                dto.CpuUsagePercentage = await GetCpuUsageWindowsAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                dto.CpuName = await GetCpuNameLinuxAsync();
                var ramInfo = await GetRamInfoLinuxAsync();
                dto.TotalRamMb = ramInfo.Total;
                dto.UsedRamMb = ramInfo.Used;
                dto.RamUsagePercentage = ramInfo.Percentage;
                dto.CpuUsagePercentage = await GetCpuUsageLinuxAsync();
            }
            else
            {
                _logger.LogWarning("System information is not supported on this OS platform.");
            }

            return dto;
        }

        private string GetCpuNameWindows()
        {
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return key?.GetValue("ProcessorNameString")?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CPU name on Windows.");
                return "Unknown";
            }
        }

        private async Task<string> GetCpuNameLinuxAsync()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync("/proc/cpuinfo");
                return lines.FirstOrDefault(line => line.StartsWith("model name"))?.Split(':')[1].Trim() ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CPU name on Linux.");
                return "Unknown";
            }
        }

        private (ulong Total, ulong Used, double Percentage) GetRamInfoWindows()
        {
            try
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    var totalMb = memStatus.ullTotalPhys / (1024 * 1024);
                    var availableMb = memStatus.ullAvailPhys / (1024 * 1024);
                    var usedMb = totalMb - availableMb;
                    var percentage = totalMb > 0 ? (double)usedMb / totalMb * 100 : 0;
                    return (totalMb, usedMb, Math.Round(percentage, 2));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get RAM info on Windows.");
            }

            return (0, 0, 0);
        }

        private async Task<(ulong Total, ulong Used, double Percentage)> GetRamInfoLinuxAsync()
        {
            try
            {
                var memInfoLines = await File.ReadAllLinesAsync("/proc/meminfo");
                var memTotalLine = memInfoLines.FirstOrDefault(l => l.StartsWith("MemTotal:"));
                var memAvailableLine = memInfoLines.FirstOrDefault(l => l.StartsWith("MemAvailable:"));

                if (memTotalLine == null || memAvailableLine == null)
                    throw new InvalidDataException("Missing /proc/meminfo lines");

                var totalKb = ulong.Parse(memTotalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                var availableKb = ulong.Parse(memAvailableLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

                var totalMb = totalKb / 1024;
                var availableMb = availableKb / 1024;
                var usedMb = totalMb - availableMb;
                var percentage = totalMb > 0 ? (double)usedMb / totalMb * 100 : 0;

                return (totalMb, usedMb, Math.Round(percentage, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get RAM info on Linux.");
                return (0, 0, 0);
            }
        }

        private async Task<double> GetCpuUsageWindowsAsync()
        {
            try
            {
                using var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                counter.NextValue();
                await Task.Delay(1000);
                return Math.Round(counter.NextValue(), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CPU usage on Windows.");
                return 0;
            }
        }

        private async Task<double> GetCpuUsageLinuxAsync()
        {
            try
            {
                var stat1 = await GetCpuTimesLinuxAsync();
                await Task.Delay(1000);
                var stat2 = await GetCpuTimesLinuxAsync();

                var total1 = stat1.Values.Select(v => (long)v).Sum();
                var total2 = stat2.Values.Select(v => (long)v).Sum();

                var idle1 = stat1.GetValueOrDefault<string, ulong>("idle", 0);
                var idle2 = stat2.GetValueOrDefault<string, ulong>("idle", 0);

                var totalDiff = total2 - total1;
                var idleDiff = idle2 - idle1;

                return totalDiff == 0 ? 0 : Math.Round((1.0 - (double)idleDiff / totalDiff) * 100, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CPU usage on Linux.");
                return 0;
            }
        }

        private async Task<Dictionary<string, ulong>> GetCpuTimesLinuxAsync()
        {
            var lines = await File.ReadAllLinesAsync("/proc/stat");
            var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
            if (cpuLine == null)
                return new Dictionary<string, ulong>();

            var values = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return new Dictionary<string, ulong>
            {
                ["user"] = ulong.Parse(values[1]),
                ["nice"] = ulong.Parse(values[2]),
                ["system"] = ulong.Parse(values[3]),
                ["idle"] = ulong.Parse(values[4]),
                ["iowait"] = values.Length > 5 ? ulong.Parse(values[5]) : 0,
                ["irq"] = values.Length > 6 ? ulong.Parse(values[6]) : 0,
                ["softirq"] = values.Length > 7 ? ulong.Parse(values[7]) : 0,
                ["steal"] = values.Length > 8 ? ulong.Parse(values[8]) : 0,
                ["guest"] = values.Length > 9 ? ulong.Parse(values[9]) : 0,
                ["guest_nice"] = values.Length > 10 ? ulong.Parse(values[10]) : 0
            };
        }

        private List<DriveInfoDto> GetDriveInfo()
        {
            return DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d =>
                {
                    var totalSizeGb = d.TotalSize / (1024.0 * 1024 * 1024);
                    var availableSpaceGb = d.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                    var usedSpaceGb = totalSizeGb - availableSpaceGb;
                    var usagePercentage = totalSizeGb > 0 ? (usedSpaceGb / totalSizeGb) * 100 : 0;

                    return new DriveInfoDto
                    {
                        Name = d.Name,
                        DriveFormat = d.DriveFormat,
                        TotalSizeGb = Math.Round(totalSizeGb, 2),
                        AvailableFreeSpaceGb = Math.Round(availableSpaceGb, 2),
                        UsedSpaceGb = Math.Round(usedSpaceGb, 2),
                        UsagePercentage = Math.Round(usagePercentage, 2)
                    };
                }).ToList();
        }

        // Native struct and P/Invoke
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}
