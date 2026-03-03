namespace DevNotes.Domain;

/// <summary>
/// 表示一篇开发笔记/文章的领域模型。
/// 该模型仅关注笔记本身的数据结构，不包含任何 UI 或持久化实现细节。
/// </summary>
public class Note
{
    /// <summary>
    /// 笔记的唯一标识。
    /// 将来存入数据库后可与主键对应，在纯内存模式下也可用于列表区分。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 笔记标题，用于列表展示和快速识别笔记内容。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 笔记正文内容，使用 Markdown 文本进行存储。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 笔记的创建时间，用于时间轴、排序等场景。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 笔记的最后一次更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否置顶，用于在列表中进行优先展示。
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否已归档，用于从日常列表中隐藏但仍保留记录。
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// 是否已删除，用于实现回收站逻辑（逻辑删除）。
    /// </summary>
    public bool IsDeleted { get; set; }
}
