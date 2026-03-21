using System.Data;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure.Repositories;

/// <summary>
/// 基于 SQLite 的文章数据访问实现。
/// </summary>
public class SqliteArticleRepository : IArticleRepository
{
    private readonly string _connectionString;

    public SqliteArticleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<Article> GetAll(ArticleStatus? status = null)
    {
        var articles = new List<Article>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        var sql = @"SELECT a.Id, a.Title, a.Slug, a.Content, a.Summary, a.CoverImagePath,
                          a.Status, a.CreatedAt, a.UpdatedAt, a.PublishedAt, a.IsDeleted, a.CategoryId,
                          c.Name as CategoryName
                   FROM Articles a
                   LEFT JOIN Categories c ON a.CategoryId = c.Id
                   WHERE a.IsDeleted = 0";

        if (status.HasValue)
        {
            sql += " AND a.Status = $status";
            command.Parameters.AddWithValue("$status", (int)status.Value);
        }

        sql += " ORDER BY a.UpdatedAt DESC";
        command.CommandText = sql;

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            var article = new Article
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Slug = reader.GetString(2),
                Content = reader.GetString(3),
                Summary = reader.GetString(4),
                CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                Status = (ArticleStatus)reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8),
                PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                IsDeleted = reader.GetBoolean(10),
                CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };

            if (!reader.IsDBNull(11) && !reader.IsDBNull(12))
            {
                article.Category = new Category
                {
                    Id = reader.GetInt32(11),
                    Name = reader.GetString(12)
                };
            }

            articles.Add(article);
        }

        return articles;
    }

    public Article? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT a.Id, a.Title, a.Slug, a.Content, a.Summary, a.CoverImagePath,
                     a.Status, a.CreatedAt, a.UpdatedAt, a.PublishedAt, a.IsDeleted, a.CategoryId,
                     c.Name as CategoryName
              FROM Articles a
              LEFT JOIN Categories c ON a.CategoryId = c.Id
              WHERE a.Id = $id AND a.IsDeleted = 0";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        var article = new Article
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Slug = reader.GetString(2),
            Content = reader.GetString(3),
            Summary = reader.GetString(4),
            CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
            Status = (ArticleStatus)reader.GetInt32(6),
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.GetDateTime(8),
            PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
            IsDeleted = reader.GetBoolean(10),
            CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
        };

        if (!reader.IsDBNull(11) && !reader.IsDBNull(12))
        {
            article.Category = new Category
            {
                Id = reader.GetInt32(11),
                Name = reader.GetString(12)
            };
        }

        return article;
    }

    public Article? GetBySlug(string slug)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Title, Slug, Content, Summary, CoverImagePath,
                     Status, CreatedAt, UpdatedAt, PublishedAt, IsDeleted, CategoryId
              FROM Articles
              WHERE Slug = $slug AND IsDeleted = 0";

        command.Parameters.AddWithValue("$slug", slug);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        return new Article
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Slug = reader.GetString(2),
            Content = reader.GetString(3),
            Summary = reader.GetString(4),
            CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
            Status = (ArticleStatus)reader.GetInt32(6),
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.GetDateTime(8),
            PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
            IsDeleted = reader.GetBoolean(10),
            CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
        };
    }

    public void Add(Article article)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO Articles (Title, Slug, Content, Summary, CoverImagePath,
                                    Status, CreatedAt, UpdatedAt, PublishedAt, IsDeleted, CategoryId)
              VALUES ($title, $slug, $content, $summary, $coverImagePath,
                      $status, $createdAt, $updatedAt, $publishedAt, $isDeleted, $categoryId);
              SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$title", article.Title);
        command.Parameters.AddWithValue("$slug", article.Slug);
        command.Parameters.AddWithValue("$content", article.Content);
        command.Parameters.AddWithValue("$summary", article.Summary);
        command.Parameters.AddWithValue("$coverImagePath", (object?)article.CoverImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", (int)article.Status);
        command.Parameters.AddWithValue("$createdAt", article.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", article.UpdatedAt);
        command.Parameters.AddWithValue("$publishedAt", (object?)article.PublishedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("$isDeleted", article.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$categoryId", (object?)article.CategoryId ?? DBNull.Value);

        var result = command.ExecuteScalar();
        if (result is long id)
        {
            article.Id = (int)id;
        }
    }

    public void Update(Article article)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Articles
              SET Title = $title,
                  Slug = $slug,
                  Content = $content,
                  Summary = $summary,
                  CoverImagePath = $coverImagePath,
                  Status = $status,
                  UpdatedAt = $updatedAt,
                  PublishedAt = $publishedAt,
                  IsDeleted = $isDeleted,
                  CategoryId = $categoryId
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$title", article.Title);
        command.Parameters.AddWithValue("$slug", article.Slug);
        command.Parameters.AddWithValue("$content", article.Content);
        command.Parameters.AddWithValue("$summary", article.Summary);
        command.Parameters.AddWithValue("$coverImagePath", (object?)article.CoverImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", (int)article.Status);
        command.Parameters.AddWithValue("$updatedAt", article.UpdatedAt);
        command.Parameters.AddWithValue("$publishedAt", (object?)article.PublishedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("$isDeleted", article.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$categoryId", (object?)article.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", article.Id);

        command.ExecuteNonQuery();
    }

    public void Delete(int articleId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Articles
              SET IsDeleted = 1
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$id", articleId);
        command.ExecuteNonQuery();
    }

    public void Restore(int articleId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Articles
              SET IsDeleted = 0
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$id", articleId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Article> Search(string keyword)
    {
        var articles = new List<Article>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Title, Slug, Content, Summary, CoverImagePath,
                     Status, CreatedAt, UpdatedAt, PublishedAt, IsDeleted, CategoryId
              FROM Articles
              WHERE IsDeleted = 0
                AND (Title LIKE $keyword OR Content LIKE $keyword)
              ORDER BY UpdatedAt DESC";

        command.Parameters.AddWithValue("$keyword", $"%{keyword}%");

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            var article = new Article
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Slug = reader.GetString(2),
                Content = reader.GetString(3),
                Summary = reader.GetString(4),
                CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                Status = (ArticleStatus)reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8),
                PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                IsDeleted = reader.GetBoolean(10),
                CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };

            articles.Add(article);
        }

        return articles;
    }

    public IReadOnlyList<Article> GetByCategory(int categoryId)
    {
        var articles = new List<Article>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Title, Slug, Content, Summary, CoverImagePath,
                     Status, CreatedAt, UpdatedAt, PublishedAt, IsDeleted, CategoryId
              FROM Articles
              WHERE IsDeleted = 0 AND CategoryId = $categoryId
              ORDER BY UpdatedAt DESC";

        command.Parameters.AddWithValue("$categoryId", categoryId);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            var article = new Article
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Slug = reader.GetString(2),
                Content = reader.GetString(3),
                Summary = reader.GetString(4),
                CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                Status = (ArticleStatus)reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8),
                PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                IsDeleted = reader.GetBoolean(10),
                CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };

            articles.Add(article);
        }

        return articles;
    }

    public IReadOnlyList<Article> GetByTag(int tagId)
    {
        var articles = new List<Article>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT a.Id, a.Title, a.Slug, a.Content, a.Summary, a.CoverImagePath,
                     a.Status, a.CreatedAt, a.UpdatedAt, a.PublishedAt, a.IsDeleted, a.CategoryId
              FROM Articles a
              INNER JOIN ArticleTags at ON a.Id = at.ArticleId
              WHERE a.IsDeleted = 0 AND at.TagId = $tagId
              ORDER BY a.UpdatedAt DESC";

        command.Parameters.AddWithValue("$tagId", tagId);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            var article = new Article
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Slug = reader.GetString(2),
                Content = reader.GetString(3),
                Summary = reader.GetString(4),
                CoverImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                Status = (ArticleStatus)reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8),
                PublishedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                IsDeleted = reader.GetBoolean(10),
                CategoryId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };

            articles.Add(article);
        }

        return articles;
    }
}
