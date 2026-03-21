namespace DevNotes.Domain.Models;

/// <summary>
/// 文章状态枚举。
/// </summary>
public enum ArticleStatus
{
    /// <summary>
    /// 草稿状态，尚未发布。
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发布状态。
    /// </summary>
    Published = 1
}
