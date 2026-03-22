using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DevNotes.Infrastructure.Services;

/// <summary>
/// 本地图片存储服务实现。
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    /// <summary>
    /// 存储图片并返回附件信息。
    /// </summary>
    public ImageAttachment SaveImage(string originalFileName, byte[] imageData)
    {
        var now = DateTime.Now;
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        // 验证文件扩展名
        if (!IsValidImageExtension(extension))
        {
            throw new ArgumentException($"不支持的图片格式: {extension}");
        }

        // 生成唯一文件名
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var storagePath = $"{now.Year}/{now.Month:D2}/{uniqueFileName}";

        // 获取存储目录
        var imagesDir = AppDataPaths.GetImagesDirectory(now.Year, now.Month);
        var fullPath = Path.Combine(imagesDir, uniqueFileName);

        // 保存图片
        File.WriteAllBytes(fullPath, imageData);

        // 获取图片尺寸
        int width = 0, height = 0;
        try
        {
            using var image = Image.Load(imageData);
            width = image.Width;
            height = image.Height;
        }
        catch
        {
            // 如果无法读取图片尺寸，保持默认值
        }

        return new ImageAttachment
        {
            OriginalFileName = originalFileName,
            StoragePath = storagePath,
            FileSize = imageData.Length,
            Width = width,
            Height = height,
            CreatedAt = now
        };
    }

    /// <summary>
    /// 删除图片文件。
    /// </summary>
    public void DeleteImage(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return;
        }

        var fullPath = GetFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    /// <summary>
    /// 获取图片完整路径。
    /// </summary>
    public string GetFullPath(string storagePath)
    {
        return Path.Combine(AppDataPaths.GetImagesDirectory(), storagePath);
    }

    /// <summary>
    /// 验证文件扩展名是否为支持的图片格式。
    /// </summary>
    private static bool IsValidImageExtension(string extension)
    {
        var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp" };
        return validExtensions.Contains(extension);
    }
}
