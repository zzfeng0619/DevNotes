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
        // AppContext.BaseDirectory 一般指向 WPF 程序的 bin\Debug\net10.0-windows 目录。
        // 通过向上回退若干级目录，再进入 data\db 子目录以兼容当前解决方案布局。
        var baseDir = AppContext.BaseDirectory;
        var solutionRootCandidate = Path.GetFullPath(
            Path.Combine(baseDir, "..", "..", "..", ".."));

        var dbDirectory = Path.Combine(solutionRootCandidate, "data", "db");

        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        return Path.Combine(dbDirectory, "notes.db");
    }
}

