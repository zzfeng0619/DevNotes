using DevNotes.Domain.Models;

namespace DevNotes.Domain.Interfaces;

/// <summary>
/// 图片附件数据访问接口。
/// </summary>
public interface IImageAttachmentRepository
{
    /// <summary>
    /// 获取文章的所有图片附件。
    /// </summary>
    /// <param name="articleId">文章 ID。</param>
    /// <returns>图片附件列表。</returns>
    IReadOnlyList<ImageAttachment> GetByArticle(int articleId);

    /// <summary>
    /// 根据 ID 获取图片附件。
    /// </summary>
    /// <param name="id">图片附件 ID。</param>
    /// <returns>图片附件实例，不存在时返回 null。</returns>
    ImageAttachment? GetById(int id);

    /// <summary>
    /// 新增图片附件。
    /// </summary>
    /// <param name="image">图片附件实例。</param>
    void Add(ImageAttachment image);

    /// <summary>
    /// 更新图片附件（仅更新 AltText）。
    /// </summary>
    /// <param name="image">图片附件实例。</param>
    void Update(ImageAttachment image);

    /// <summary>
    /// 删除图片附件。
    /// </summary>
    /// <param name="id">图片附件 ID。</param>
    void Delete(int id);
}
