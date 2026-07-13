namespace Docker_Mcp.Utils;

public static class OutputFormatter
{

    public const string Success = "✅";
    public const string Error = "❌";
    public const string Info = "ℹ️";
    public const string Warning = "⚠️";

    public static string FormatSize(long bytes)
    {
        return bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F2} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F2} MB",
            >= 1_024 => $"{bytes / 1_024.0:F2} KB",
            _ => $"{bytes} B"
        };
    }

    public static string FormatContainer(string shortId, string name, string image, string stateOrStatus)
    {
        return $"ID: {shortId} | Name: {name} | Image: {image} | {stateOrStatus}";
    }

    public static string FormatImage(string shortId, string tags, long sizeBytes)
    {
        return $"ID: {shortId} | Tags: {tags} | Size: {FormatSize(sizeBytes)}";
    }


    public static string FormatVolume(string name, string driver, string mountpoint)
    {
        return $"Name: {name} | Driver: {driver} | Mountpoint: {mountpoint}";
    }
}
