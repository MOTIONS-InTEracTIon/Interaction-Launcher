public static class SizeFormatter
{
    public static string FormatSize(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        if (bytes >= gb)
        {
            return (bytes / (float)gb).ToString("0.00") + " GB";
        }
        else if (bytes >= mb)
        {
            return (bytes / (float)mb).ToString("0.00") + " MB";
        }
        else if (bytes >= kb)
        {
            return (bytes / (float)kb).ToString("0.00") + " KB";
        }
        else
        {
            return bytes + " bytes";
        }
    }
}