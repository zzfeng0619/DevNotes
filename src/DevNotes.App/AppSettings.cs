using System.IO;
using System.Text.Json;
using DevNotes.Infrastructure;

namespace DevNotes.App;

/// <summary>
/// 表示与界面行为相关的用户设置。
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 是否在编辑区域上方显示 Markdown 语法提示与快捷按钮。
    /// </summary>
    public bool ShowMarkdownHints { get; set; } = true;

    /// <summary>
    /// 自动保存间隔（秒），0 表示禁用自动保存。
    /// </summary>
    public int AutoSaveIntervalSeconds { get; set; } = 30;
}

/// <summary>
/// 负责从本地配置文件读取和保存应用设置的服务类。
/// </summary>
public static class SettingsService
{
    /// <summary>
    /// 从本地配置文件加载设置。
    /// </summary>
    /// <returns>加载到的设置实例。</returns>
    public static AppSettings Load()
    {
        try
        {
            var path = AppDataPaths.GetSettingsFilePath();
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// 将给定的设置对象持久化到本地配置文件。
    /// </summary>
    /// <param name="settings">要保存的设置实例。</param>
    public static void Save(AppSettings settings)
    {
        try
        {
            var path = AppDataPaths.GetSettingsFilePath();
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
        catch
        {
            // 静默忽略持久化失败
        }
    }
}
