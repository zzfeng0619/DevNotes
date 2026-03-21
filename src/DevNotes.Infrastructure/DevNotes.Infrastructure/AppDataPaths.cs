using System.IO;

namespace DevNotes.Infrastructure;

/// <summary>
/// 统一管理应用运行时使用的本地路径，例如数据库文件、图片存储和资源目录。
/// </summary>
public static class AppDataPaths
{
    /// <summary>
    /// 获取 SQLite 数据库文件的完整路径。
    /// </summary>
    public static string GetDatabaseFilePath()
    {
        var dbDirectory = Path.Combine(GetSolutionRootPath(), "data", "db");

        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        return Path.Combine(dbDirectory, "blog.db");
    }

    /// <summary>
    /// 获取图片存储根目录。
    /// </summary>
    public static string GetImagesDirectory()
    {
        var imagesDirectory = Path.Combine(GetSolutionRootPath(), "data", "images");

        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }

        return imagesDirectory;
    }

    /// <summary>
    /// 获取指定年月的图片存储目录。
    /// </summary>
    /// <param name="year">年份。</param>
    /// <param name="month">月份。</param>
    /// <returns>目录完整路径。</returns>
    public static string GetImagesDirectory(int year, int month)
    {
        var directory = Path.Combine(GetImagesDirectory(), year.ToString(), month.ToString("D2"));

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    /// <summary>
    /// 获取缩略图存储目录。
    /// </summary>
    public static string GetThumbnailsDirectory()
    {
        var thumbnailsDirectory = Path.Combine(GetImagesDirectory(), "thumbnails");

        if (!Directory.Exists(thumbnailsDirectory))
        {
            Directory.CreateDirectory(thumbnailsDirectory);
        }

        return thumbnailsDirectory;
    }

    /// <summary>
    /// 获取附件存储目录。
    /// </summary>
    public static string GetAttachmentsDirectory()
    {
        var attachmentsDirectory = Path.Combine(GetSolutionRootPath(), "data", "attachments");

        if (!Directory.Exists(attachmentsDirectory))
        {
            Directory.CreateDirectory(attachmentsDirectory);
        }

        return attachmentsDirectory;
    }

    /// <summary>
    /// 获取用于保存应用设置的配置文件路径。
    /// </summary>
    public static string GetSettingsFilePath()
    {
        var configDirectory = Path.Combine(GetSolutionRootPath(), "data", "config");

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        return Path.Combine(configDirectory, "settings.json");
    }

    /// <summary>
    /// 计算解决方案根目录的路径。
    /// </summary>
    /// <returns>解决方案根目录的完整路径。</returns>
    private static string GetSolutionRootPath()
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }
}
