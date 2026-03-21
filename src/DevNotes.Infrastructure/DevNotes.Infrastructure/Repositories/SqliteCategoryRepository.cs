using System.Data;
using DevNotes.Domain.Interfaces;
using DevNotes.Domain.Models;
using Microsoft.Data.Sqlite;

namespace DevNotes.Infrastructure.Repositories;

/// <summary>
/// 基于 SQLite 的分类数据访问实现。
/// </summary>
public class SqliteCategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;

    public SqliteCategoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<Category> GetAll()
    {
        var categories = new List<Category>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Name, Slug, Description, SortOrder
              FROM Categories
              ORDER BY SortOrder, Name";

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        while (reader.Read())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Slug = reader.GetString(2),
                Description = reader.GetString(3),
                SortOrder = reader.GetInt32(4)
            });
        }

        return categories;
    }

    public Category? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT Id, Name, Slug, Description, SortOrder
              FROM Categories
              WHERE Id = $id";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        if (!reader.Read())
        {
            return null;
        }

        return new Category
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2),
            Description = reader.GetString(3),
            SortOrder = reader.GetInt32(4)
        };
    }

    public void Add(Category category)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO Categories (Name, Slug, Description, SortOrder)
              VALUES ($name, $slug, $description, $sortOrder);
              SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$slug", category.Slug);
        command.Parameters.AddWithValue("$description", category.Description);
        command.Parameters.AddWithValue("$sortOrder", category.SortOrder);

        var result = command.ExecuteScalar();
        if (result is long id)
        {
            category.Id = (int)id;
        }
    }

    public void Update(Category category)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Categories
              SET Name = $name,
                  Slug = $slug,
                  Description = $description,
                  SortOrder = $sortOrder
              WHERE Id = $id;";

        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$slug", category.Slug);
        command.Parameters.AddWithValue("$description", category.Description);
        command.Parameters.AddWithValue("$sortOrder", category.SortOrder);
        command.Parameters.AddWithValue("$id", category.Id);

        command.ExecuteNonQuery();
    }

    public void Delete(int categoryId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Categories WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", categoryId);

        command.ExecuteNonQuery();
    }
}
