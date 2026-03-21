using System.Collections.Generic;
using DevNotes.Domain.Models;

namespace DevNotes.Domain.Interfaces;

/// <summary>
/// 文章数据访问接口。
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// 获取所有未删除的文章。
    /// </summary>
    /// <param name="status">可选的状态筛选。</param>
    /// <returns>文章列表。</returns>
    IReadOnlyList<Article> GetAll(ArticleStatus? status = null);

    /// <summary>
    /// 根据 ID 获取文章。
    /// </summary>
    /// <param name="id">文章 ID。</param>
    /// <returns>文章实例，不存在时返回 null。</returns>
    Article? GetById(int id);

    /// <summary>
    /// 根据 Slug 获取文章。
    /// </summary>
    /// <param name="slug">URL 标识。</param>
    /// <returns>文章实例，不存在时返回 null。</returns>
    Article? GetBySlug(string slug);

    /// <summary>
    /// 新增文章。
    /// </summary>
    /// <param name="article">文章实例。</param>
    void Add(Article article);

    /// <summary>
    /// 更新文章。
    /// </summary>
    /// <param name="article">文章实例。</param>
    void Update(Article article);

    /// <summary>
    /// 逻辑删除文章。
    /// </summary>
    /// <param name="articleId">文章 ID。</param>
    void Delete(int articleId);

    /// <summary>
    /// 恢复已删除的文章。
    /// </summary>
    /// <param name="articleId">文章 ID。</param>
    void Restore(int articleId);

    /// <summary>
    /// 按关键词搜索文章（标题和内容）。
    /// </summary>
    /// <param name="keyword">搜索关键词。</param>
    /// <returns>匹配的文章列表。</returns>
    IReadOnlyList<Article> Search(string keyword);

    /// <summary>
    /// 按分类获取文章。
    /// </summary>
    /// <param name="categoryId">分类 ID。</param>
    /// <returns>文章列表。</returns>
    IReadOnlyList<Article> GetByCategory(int categoryId);

    /// <summary>
    /// 按标签获取文章。
    /// </summary>
    /// <param name="tagId">标签 ID。</param>
    /// <returns>文章列表。</returns>
    IReadOnlyList<Article> GetByTag(int tagId);
}
