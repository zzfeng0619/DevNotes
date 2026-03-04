using System.IO;

namespace DevNotes.Infrastructure;

/// <summary>
/// 统一管理应用运行时使用的本地路径，例如数据库文件和资源目录。
/// 当前实现基于开发环境的目录结构，后续可根据发布形态进行调整。
/// </summary>
public static class AppDataPaths
{
    /// <summary>
    /// 获取 SQLite 数据库文件的完整路径。
    /// 在当前项目结构下，会定位到解决方案根目录下的 data\db\notes.db。
    /// </summary>
    public static string GetDatabaseFilePath()
    {
        var dbDirectory = Path.Combine(GetSolutionRootPath(), "data", "db");

        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        return Path.Combine(dbDirectory, "notes.db");
    }

    /// <summary>
    /// 获取用于保存 UI 设置的配置文件路径。
    /// 在当前项目结构下，会定位到解决方案根目录下的 data\config\ui-settings.json。
    /// </summary>
    public static string GetUiSettingsFilePath()
    {
        var configDirectory = Path.Combine(GetSolutionRootPath(), "data", "config");

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        return Path.Combine(configDirectory, "ui-settings.json");
    }

    /// <summary>
    /// 计算解决方案根目录的路径。
    /// 当前实现基于运行时目录向上回退固定层级。
    /// </summary>
    /// <returns>解决方案根目录的完整路径。</returns>
    private static string GetSolutionRootPath()
    {
        // AppContext.BaseDirectory 一般指向 WPF 程序的 bin\Debug\net10.0-windows 目录。
        // 通过向上回退若干级目录，再进入 data 子目录以兼容当前解决方案布局。
        var baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }
}

