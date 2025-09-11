namespace Infrastructure.Utilities;

public class SystemResources
{
    public static long GetTotalFreeSpace(string driveName)
    {
        foreach (var drive in DriveInfo.GetDrives())
            if (drive.IsReady && string.Equals(drive.Name, driveName, StringComparison.OrdinalIgnoreCase))
                return drive.TotalFreeSpace;

        throw new InvalidDataException("No drive with such name was found.");
    }

    public static long GetCurrentDriveFreeSpace()
    {
        var appPath = AppContext.BaseDirectory;

        var root = Path.GetPathRoot(appPath);

        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("Failed to determine drive root from app path.");

        return GetTotalFreeSpace(root);
    }
}