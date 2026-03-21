using System.Collections.Generic;
using DevNotes.Domain.Models;

namespace DevNotes.Domain.Interfaces;

/// <summary>
/// 分类数据访问接口。
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// 获取所有分类。
    /// </summary>
    /// <returns>分类列表。</returns>
    IReadOnlyList<Category> GetAll();

    /// <summary>
    /// 根据 ID 获取分类。
    /// </summary>
    /// <param name="id">分类 ID。</param>
    /// <returns>分类实例，不存在时返回 null。</returns>
    Category? GetById(int id);

    /// <summary>
    /// 新增分类。
    /// </summary>
    /// <param name="category">分类实例。</param>
    void Add(Category category);

    /// <summary>
    /// 更新分类。
    /// </summary>
    /// <param name="category">分类实例。</param>
    void Update(Category category);

    /// <summary>
    /// 删除分类。
    /// </summary>
    /// <param name="categoryId">分类 ID。</param>
    void Delete(int categoryId);
}
