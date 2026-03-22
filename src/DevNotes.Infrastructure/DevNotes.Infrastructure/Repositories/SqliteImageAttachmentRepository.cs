using System.Data;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure.Repositories;

/// <summary>
/// 基于 SQLite 的图片附件数据访问实现。
/// </summary>
public class SqliteImageAttachmentRepository : IImageAttachmentRepository
{
    private readonly string _connectionString;

    public SqliteImageAttachmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<ImageAttachment> GetByArticle(int articleId)
    {
        var images = new List<ImageAttachment>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, ArticleId, OriginalFileName, StoragePath, AltText, FileSize, Width, Height, CreatedAt
              FROM ImageAttachments
              WHERE ArticleId = $articleId
              ORDER BY CreatedAt DESC";

        command.Parameters.AddWithValue("$articleId", articleId);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            images.Add(new ImageAttachment
            {
                Id = reader.GetInt32(0),
                ArticleId = reader.GetInt32(1),
                OriginalFileName = reader.GetString(2),
                StoragePath = reader.GetString(3),
                AltText = reader.GetString(4),
                FileSize = reader.GetInt64(5),
                Width = reader.GetInt32(6),
                Height = reader.GetInt32(7),
                CreatedAt = reader.GetDateTime(8)
            });
        }

        return images;
    }

    public ImageAttachment? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, ArticleId, OriginalFileName, StoragePath, AltText, FileSize, Width, Height, CreatedAt
              FROM ImageAttachments
              WHERE Id = $id";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        return new ImageAttachment
        {
            Id = reader.GetInt32(0),
            ArticleId = reader.GetInt32(1),
            OriginalFileName = reader.GetString(2),
            StoragePath = reader.GetString(3),
            AltText = reader.GetString(4),
            FileSize = reader.GetInt64(5),
            Width = reader.GetInt32(6),
            Height = reader.GetInt32(7),
            CreatedAt = reader.GetDateTime(8)
        };
    }

    public void Add(ImageAttachment image)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO ImageAttachments (ArticleId, OriginalFileName, StoragePath, AltText, FileSize, Width, Height, CreatedAt)
              VALUES ($articleId, $originalFileName, $storagePath, $altText, $fileSize, $width, $height, $createdAt);
              SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$articleId", image.ArticleId);
        command.Parameters.AddWithValue("$originalFileName", image.OriginalFileName);
        command.Parameters.AddWithValue("$storagePath", image.StoragePath);
        command.Parameters.AddWithValue("$altText", image.AltText);
        command.Parameters.AddWithValue("$fileSize", image.FileSize);
        command.Parameters.AddWithValue("$width", image.Width);
        command.Parameters.AddWithValue("$height", image.Height);
        command.Parameters.AddWithValue("$createdAt", image.CreatedAt);

        var result = command.ExecuteScalar();
        if (result is long id)
        {
            image.Id = (int)id;
        }
    }

    public void Update(ImageAttachment image)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE ImageAttachments
              SET AltText = $altText
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$altText", image.AltText);
        command.Parameters.AddWithValue("$id", image.Id);

        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM ImageAttachments WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        command.ExecuteNonQuery();
    }
}
