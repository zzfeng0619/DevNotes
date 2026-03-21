using System.Collections.Generic;
using DevNotes.Domain.Models;

namespace DevNotes.Domain.Interfaces;

/// <summary>
/// 标签数据访问接口。
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// 获取所有标签。
    /// </summary>
    /// <returns>标签列表。</returns>
    IReadOnlyList<Tag> GetAll();

    /// <summary>
    /// 根据 ID 获取标签。
    /// </summary>
    /// <param name="id">标签 ID。</param>
    /// <returns>标签实例，不存在时返回 null。</returns>
    Tag? GetById(int id);

    /// <summary>
    /// 根据名称获取标签。
    /// </summary>
    /// <param name="name">标签名称。</param>
    /// <returns>标签实例，不存在时返回 null。</returns>
    Tag? GetByName(string name);

    /// <summary>
    /// 新增标签。
    /// </summary>
    /// <param name="tag">标签实例。</param>
    void Add(Tag tag);

    /// <summary>
    /// 删除标签。
    /// </summary>
    /// <param name="tagId">标签 ID。</param>
    void Delete(int tagId);

    /// <summary>
    /// 更新文章关联的标签。
    /// </summary>
    /// <param name="articleId">文章 ID。</param>
    /// <param name="tagIds">标签 ID 集合。</param>
    void UpdateArticleTags(int articleId, IEnumerable<int> tagIds);

    /// <summary>
    /// 获取文章关联的标签。
    /// </summary>
    /// <param name="articleId">文章 ID。</param>
    /// <returns>标签列表。</returns>
    IReadOnlyList<Tag> GetByArticle(int articleId);
}
