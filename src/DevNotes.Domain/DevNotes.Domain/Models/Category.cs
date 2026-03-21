namespace DevNotes.Domain.Models;

/// <summary>
/// 文章分类领域模型。
/// </summary>
public class Category
{
    /// <summary>
    /// 分类唯一标识。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分类名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL 友好标识。
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 分类描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 排序权重，数值越小越靠前。
    /// </summary>
    public int SortOrder { get; set; }
}
