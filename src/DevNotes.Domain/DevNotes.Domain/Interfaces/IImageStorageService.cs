using DevNotes.Domain.Models;

namespace DevNotes.Domain.Interfaces;

/// <summary>
/// 图片存储服务接口。
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// 存储图片并返回附件信息。
    /// </summary>
    /// <param name="originalFileName">原始文件名。</param>
    /// <param name="imageData">图片二进制数据。</param>
    /// <returns>图片附件信息。</returns>
    ImageAttachment SaveImage(string originalFileName, byte[] imageData);

    /// <summary>
    /// 删除图片文件。
    /// </summary>
    /// <param name="storagePath">存储相对路径。</param>
    void DeleteImage(string storagePath);

    /// <summary>
    /// 获取图片完整路径。
    /// </summary>
    /// <param name="storagePath">存储相对路径。</param>
    /// <returns>完整文件路径。</returns>
    string GetFullPath(string storagePath);
}
