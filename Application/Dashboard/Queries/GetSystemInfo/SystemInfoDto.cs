using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dashboard.Queries.GetSystemInfo
{

    public class SystemInfoDto
    {
        public string OsDescription { get; set; }
        public string FrameworkDescription { get; set; }
        public string CpuName { get; set; }
        public int CpuCoreCount { get; set; }
        public double CpuUsagePercentage { get; set; }
        public ulong TotalRamMb { get; set; }
        public ulong UsedRamMb { get; set; }
        public double RamUsagePercentage { get; set; }
        public List<DriveInfoDto> Drives { get; set; }
    }

    public class DriveInfoDto
    {
        public string Name { get; set; }
        public string DriveFormat { get; set; }
        public double TotalSizeGb { get; set; }
        public double AvailableFreeSpaceGb { get; set; }
        public double UsedSpaceGb { get; set; }
        public double UsagePercentage { get; set; }
    }
}

