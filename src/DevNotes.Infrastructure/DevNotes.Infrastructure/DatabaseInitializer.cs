using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure;

/// <summary>
/// 数据库初始化服务，负责创建数据库和表结构。
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// 确保数据库和所有表结构已创建。
    /// </summary>
    public static void EnsureDatabaseCreated()
    {
        var dbPath = AppDataPaths.GetDatabaseFilePath();
        var connectionString = $"Data Source={dbPath}";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            CreateTables(connection);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 获取数据库连接字符串。
    /// </summary>
    /// <returns>SQLite 连接字符串。</returns>
    public static string GetConnectionString()
    {
        var dbPath = AppDataPaths.GetDatabaseFilePath();
        return $"Data Source={dbPath}";
    }

    private static void CreateTables(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText =
            @"
            -- 分类表
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Slug TEXT NOT NULL UNIQUE,
                Description TEXT NOT NULL DEFAULT '',
                SortOrder INTEGER NOT NULL DEFAULT 0
            );

            -- 标签表
            CREATE TABLE IF NOT EXISTS Tags (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Slug TEXT NOT NULL UNIQUE
            );

            -- 文章表
            CREATE TABLE IF NOT EXISTS Articles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Slug TEXT NOT NULL UNIQUE,
                Content TEXT NOT NULL DEFAULT '',
                Summary TEXT NOT NULL DEFAULT '',
                CoverImagePath TEXT,
                Status INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                PublishedAt TEXT,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                CategoryId INTEGER,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );

            -- 文章-标签关联表
            CREATE TABLE IF NOT EXISTS ArticleTags (
                ArticleId INTEGER NOT NULL,
                TagId INTEGER NOT NULL,
                PRIMARY KEY (ArticleId, TagId),
                FOREIGN KEY (ArticleId) REFERENCES Articles(Id),
                FOREIGN KEY (TagId) REFERENCES Tags(Id)
            );

            -- 图片附件表
            CREATE TABLE IF NOT EXISTS ImageAttachments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ArticleId INTEGER NOT NULL,
                OriginalFileName TEXT NOT NULL,
                StoragePath TEXT NOT NULL,
                AltText TEXT NOT NULL DEFAULT '',
                FileSize INTEGER NOT NULL,
                Width INTEGER NOT NULL DEFAULT 0,
                Height INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ArticleId) REFERENCES Articles(Id)
            );

            -- 索引
            CREATE INDEX IF NOT EXISTS idx_articles_status ON Articles(Status, IsDeleted);
            CREATE INDEX IF NOT EXISTS idx_articles_category ON Articles(CategoryId);
            CREATE INDEX IF NOT EXISTS idx_articles_created ON Articles(CreatedAt);
            CREATE INDEX IF NOT EXISTS idx_images_article ON ImageAttachments(ArticleId);
            CREATE INDEX IF NOT EXISTS idx_articletags_tag ON ArticleTags(TagId);

            -- 插入默认分类（如果不存在）
            INSERT OR IGNORE INTO Categories (Name, Slug, Description, SortOrder) VALUES ('技术', 'tech', '技术相关文章', 1);
            INSERT OR IGNORE INTO Categories (Name, Slug, Description, SortOrder) VALUES ('生活', 'life', '生活随笔', 2);
            INSERT OR IGNORE INTO Categories (Name, Slug, Description, SortOrder) VALUES ('随笔', 'notes', '其他随想', 3);
            ";

        command.ExecuteNonQuery();
    }
}
