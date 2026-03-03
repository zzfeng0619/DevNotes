using System.Data;
using DevNotes.Domain;
using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure;

/// <summary>
/// 基于 SQLite 的笔记数据访问实现。
/// 负责创建数据库和表结构，并提供对 <see cref="Note"/> 的基本 CRUD 操作。
/// </summary>
public class SqliteNoteRepository : INoteRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// 初始化 <see cref="SqliteNoteRepository"/> 实例，并确保数据库与表结构已创建。
    /// </summary>
    public SqliteNoteRepository()
    {
        var dbPath = AppDataPaths.GetDatabaseFilePath();
        _connectionString = $"Data Source={dbPath}";

        EnsureDatabaseCreated();
    }

    /// <summary>
    /// 读取所有未逻辑删除的笔记，并按更新时间倒序排列。
    /// </summary>
    public IReadOnlyList<Note> GetAll()
    {
        var notes = new List<Note>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Title, Content, CreatedAt, UpdatedAt, IsPinned, IsArchived, IsDeleted
              FROM Notes
              WHERE IsDeleted = 0
              ORDER BY IsPinned DESC, UpdatedAt DESC;";

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            var note = new Note
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3),
                UpdatedAt = reader.GetDateTime(4),
                IsPinned = reader.GetBoolean(5),
                IsArchived = reader.GetBoolean(6),
                IsDeleted = reader.GetBoolean(7)
            };

            notes.Add(note);
        }

        return notes;
    }

    /// <summary>
    /// 向数据库中插入一条新的笔记记录，并回填自增主键。
    /// </summary>
    /// <param name="note">要保存的笔记实例。</param>
    public void Add(Note note)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt, IsPinned, IsArchived, IsDeleted)
              VALUES ($title, $content, $createdAt, $updatedAt, $isPinned, $isArchived, $isDeleted);
              SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$title", note.Title);
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$createdAt", note.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", note.UpdatedAt);
        command.Parameters.AddWithValue("$isPinned", note.IsPinned ? 1 : 0);
        command.Parameters.AddWithValue("$isArchived", note.IsArchived ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", note.IsDeleted ? 1 : 0);

        var result = command.ExecuteScalar();
        if (result is long id)
        {
            note.Id = (int)id;
        }
    }

    /// <summary>
    /// 更新数据库中已存在的笔记记录。
    /// </summary>
    /// <param name="note">包含最新数据的笔记实例。</param>
    public void Update(Note note)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Notes
              SET Title = $title,
                  Content = $content,
                  CreatedAt = $createdAt,
                  UpdatedAt = $updatedAt,
                  IsPinned = $isPinned,
                  IsArchived = $isArchived,
                  IsDeleted = $isDeleted
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$title", note.Title);
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$createdAt", note.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", note.UpdatedAt);
        command.Parameters.AddWithValue("$isPinned", note.IsPinned ? 1 : 0);
        command.Parameters.AddWithValue("$isArchived", note.IsArchived ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", note.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$id", note.Id);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// 将指定笔记标记为逻辑删除。
    /// </summary>
    /// <param name="noteId">要删除的笔记标识。</param>
    public void Delete(int noteId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Notes
              SET IsDeleted = 1
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$id", noteId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// 确保数据库文件存在并创建 Notes 表。
    /// 如表尚未创建，则会执行初始化建表脚本。
    /// </summary>
    private void EnsureDatabaseCreated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"CREATE TABLE IF NOT EXISTS Notes (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Title TEXT NOT NULL,
                  Content TEXT NOT NULL,
                  CreatedAt TEXT NOT NULL,
                  UpdatedAt TEXT NOT NULL,
                  IsPinned INTEGER NOT NULL DEFAULT 0,
                  IsArchived INTEGER NOT NULL DEFAULT 0,
                  IsDeleted INTEGER NOT NULL DEFAULT 0
              );";

        command.ExecuteNonQuery();
    }
}

