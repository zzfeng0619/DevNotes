using System.Data;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure.Repositories;

/// <summary>
/// 基于 SQLite 的标签数据访问实现。
/// </summary>
public class SqliteTagRepository : ITagRepository
{
    private readonly string _connectionString;

    public SqliteTagRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<Tag> GetAll()
    {
        var tags = new List<Tag>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT t.Id, t.Name, t.Slug, COUNT(at.ArticleId) as ArticleCount
              FROM Tags t
              LEFT JOIN ArticleTags at ON t.Id = at.TagId
              LEFT JOIN Articles a ON at.ArticleId = a.Id AND a.IsDeleted = 0
              GROUP BY t.Id, t.Name, t.Slug
              ORDER BY t.Name";

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            tags.Add(new Tag
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Slug = reader.GetString(2)
            });
        }

        return tags;
    }

    public Tag? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Name, Slug
              FROM Tags
              WHERE Id = $id";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        return new Tag
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2)
        };
    }

    public Tag? GetByName(string name)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Name, Slug
              FROM Tags
              WHERE Name = $name";

        command.Parameters.AddWithValue("$name", name);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        return new Tag
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2)
        };
    }

    public void Add(Tag tag)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO Tags (Name, Slug)
              VALUES ($name, $slug);
              SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", tag.Name);
        command.Parameters.AddWithValue("$slug", tag.Slug);

        var result = command.ExecuteScalar();
        if (result is long id)
        {
            tag.Id = (int)id;
        }
    }

    public void Delete(int tagId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            using var deleteRelations = connection.CreateCommand();
            deleteRelations.CommandText = "DELETE FROM ArticleTags WHERE TagId = $tagId;";
            deleteRelations.Parameters.AddWithValue("$tagId", tagId);
            deleteRelations.ExecuteNonQuery();

            using var deleteTag = connection.CreateCommand();
            deleteTag.CommandText = "DELETE FROM Tags WHERE Id = $tagId;";
            deleteTag.Parameters.AddWithValue("$tagId", tagId);
            deleteTag.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateArticleTags(int articleId, IEnumerable<int> tagIds)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            using var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM ArticleTags WHERE ArticleId = $articleId;";
            deleteCommand.Parameters.AddWithValue("$articleId", articleId);
            deleteCommand.ExecuteNonQuery();

            foreach (var tagId in tagIds)
            {
                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                    @"INSERT INTO ArticleTags (ArticleId, TagId)
                      VALUES ($articleId, $tagId);";
                insertCommand.Parameters.AddWithValue("$articleId", articleId);
                insertCommand.Parameters.AddWithValue("$tagId", tagId);
                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public IReadOnlyList<Tag> GetByArticle(int articleId)
    {
        var tags = new List<Tag>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT t.Id, t.Name, t.Slug
              FROM Tags t
              INNER JOIN ArticleTags at ON t.Id = at.TagId
              WHERE at.ArticleId = $articleId
              ORDER BY t.Name";

        command.Parameters.AddWithValue("$articleId", articleId);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            tags.Add(new Tag
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Slug = reader.GetString(2)
            });
        }

        return tags;
    }
}
