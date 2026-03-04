using System.IO;
using System.Text.Json;
using DevNotes.Infrastructure;

namespace DevNotes.App;

/// <summary>
/// 表示与界面行为相关的用户设置，例如是否显示 Markdown 语法提示等。
/// </summary>
public class UiSettings
{
    /// <summary>
    /// 是否在编辑区域上方显示 Markdown 语法提示与快捷按钮。
    /// </summary>
    public bool ShowMarkdownHints { get; set; } = true;
}

/// <summary>
/// 负责从本地配置文件读取和保存 <see cref="UiSettings"/> 的简单服务类。
/// </summary>
public static class UiSettingsService
{
    /// <summary>
    /// 从本地配置文件加载 UI 设置。
    /// 如果文件不存在或内容无效，将返回默认设置实例。
    /// </summary>
    /// <returns>加载到的 UI 设置对象。</returns>
    public static UiSettings Load()
    {
        try
        {
            var path = AppDataPaths.GetUiSettingsFilePath();
            if (!File.Exists(path))
            {
                return new UiSettings();
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<UiSettings>(json);
            return settings ?? new UiSettings();
        }
        catch
        {
            // 如果读取或反序列化失败，则回退到默认设置，避免影响应用启动。
            return new UiSettings();
        }
    }

    /// <summary>
    /// 将给定的 UI 设置对象持久化到本地配置文件。
    /// </summary>
    /// <param name="settings">要保存的 UI 设置实例。</param>
    public static void Save(UiSettings settings)
    {
        try
        {
            var path = AppDataPaths.GetUiSettingsFilePath();
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
        catch
        {
            // 持久化失败时静默忽略，避免影响前台用户操作。
        }
    }
}

