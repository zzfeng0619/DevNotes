using System;
using System.Collections.Generic;

namespace DevNotes.Domain.Models;

/// <summary>
/// 博客文章领域模型。
/// </summary>
public class Article
{
    /// <summary>
    /// 文章唯一标识。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 文章标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL 友好标识，用于生成静态页面路径。
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Markdown 正文内容。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 文章摘要，可自动生成或手动设置。
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// 封面图片的相对路径。
    /// </summary>
    public string? CoverImagePath { get; set; }

    /// <summary>
    /// 文章状态（草稿/已发布）。
    /// </summary>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 发布时间，仅在状态为 Published 时有效。
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 逻辑删除标记。
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 所属分类 ID。
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// 所属分类（导航属性）。
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// 文章关联的标签集合。
    /// </summary>
    public List<Tag> Tags { get; set; } = new();

    /// <summary>
    /// 文章关联的图片附件集合。
    /// </summary>
    public List<ImageAttachment> Images { get; set; } = new();

    /// <summary>
    /// 根据标题生成 URL 友好的 Slug。
    /// </summary>
    public void GenerateSlug()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            Slug = $"article-{Id}-{Guid.NewGuid():N}";
            return;
        }

        var baseSlug = Title
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("：", "-")
            .Replace("，", "-")
            .Replace("。", "-");

        Slug = $"{baseSlug}-{Guid.NewGuid():N}";
    }
}
