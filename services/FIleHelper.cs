public static class FileHelper
{
    public static string ToPhysicalPath(string url)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", url.TrimStart('/'));
    }

    public static void DeleteFile(string url)
    {
        var path = ToPhysicalPath(url);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}