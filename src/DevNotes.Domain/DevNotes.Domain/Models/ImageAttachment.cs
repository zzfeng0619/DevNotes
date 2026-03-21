using System;

namespace DevNotes.Domain.Models;

/// <summary>
/// 图片附件领域模型，用于管理博客文章中的图片资源。
/// </summary>
public class ImageAttachment
{
    /// <summary>
    /// 图片唯一标识。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属文章 ID。
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 存储相对路径（相对于 images 目录）。
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// 图片描述/替代文本。
    /// </summary>
    public string AltText { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 图片宽度（像素）。
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 图片高度（像素）。
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
