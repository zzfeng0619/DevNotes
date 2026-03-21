namespace DevNotes.Domain.Models;

/// <summary>
/// 标签领域模型。
/// </summary>
public class Tag
{
    /// <summary>
    /// 标签唯一标识。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标签名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL 友好标识。
    /// </summary>
    public string Slug { get; set; } = string.Empty;
}
