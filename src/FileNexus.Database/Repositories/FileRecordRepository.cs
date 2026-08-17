using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FileNexus.Database.Connection;
using FileNexus.Shared.Enums;
using FileNexus.Shared.Models;
using Microsoft.Data.Sqlite;

namespace FileNexus.Database.Repositories;

public sealed class FileRecordRepository : IFileRecordRepository
{
    private readonly IDatabaseInitializer _dbInitializer;

    public FileRecordRepository(IDatabaseInitializer dbInitializer)
    {
        _dbInitializer = dbInitializer;
    }

    public async Task BulkUpsertBatchAsync(IEnumerable<FileItem> items)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        string sql = @"INSERT OR REPLACE INTO file_records
            (id, workspace_id, folder_id, name, extension, category, absolute_path, size, created_at, modified_at, file_hash, is_favorite, thumbnail_status, tags)
            VALUES (@id, @ws_id, @folder_id, @name, @ext, @cat, @path, @size, @created, @modified, @hash, @fav, @thumb, @tags)";

        using var cmd = new SqliteCommand(sql, connection, transaction);
        var pId = cmd.Parameters.Add("@id", SqliteType.Text);
        var pWsId = cmd.Parameters.Add("@ws_id", SqliteType.Text);
        var pFolderId = cmd.Parameters.Add("@folder_id", SqliteType.Text);
        var pName = cmd.Parameters.Add("@name", SqliteType.Text);
        var pExt = cmd.Parameters.Add("@ext", SqliteType.Text);
        var pCat = cmd.Parameters.Add("@cat", SqliteType.Integer);
        var pPath = cmd.Parameters.Add("@path", SqliteType.Text);
        var pSize = cmd.Parameters.Add("@size", SqliteType.Integer);
        var pCreated = cmd.Parameters.Add("@created", SqliteType.Text);
        var pModified = cmd.Parameters.Add("@modified", SqliteType.Text);
        var pHash = cmd.Parameters.Add("@hash", SqliteType.Text);
        var pFav = cmd.Parameters.Add("@fav", SqliteType.Integer);
        var pThumb = cmd.Parameters.Add("@thumb", SqliteType.Integer);
        var pTags = cmd.Parameters.Add("@tags", SqliteType.Text);

        foreach (var item in items)
        {
            pId.Value = item.Id;
            pWsId.Value = item.WorkspaceId;
            pFolderId.Value = item.FolderId;
            pName.Value = item.Name;
            pExt.Value = item.Extension;
            pCat.Value = (int)item.Category;
            pPath.Value = item.AbsolutePath;
            pSize.Value = item.Size;
            pCreated.Value = item.CreatedAt.ToString("o");
            pModified.Value = item.ModifiedAt.ToString("o");
            pHash.Value = item.FileHash ?? (object)DBNull.Value;
            pFav.Value = item.IsFavorite ? 1 : 0;
            pThumb.Value = item.ThumbnailStatus;
            pTags.Value = item.Tags ?? string.Empty;

            await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteByFolderIdAsync(string folderId)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "DELETE FROM file_records WHERE folder_id = @folder_id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@folder_id", folderId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteByWorkspaceIdAsync(string workspaceId)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "DELETE FROM file_records WHERE workspace_id = @ws_id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ws_id", workspaceId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<FileItem>> SearchAsync(FileSearchQuery query)
    {
        var results = new List<FileItem>();
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        var sb = new StringBuilder("SELECT id, workspace_id, folder_id, name, extension, category, absolute_path, size, created_at, modified_at, file_hash, is_favorite, thumbnail_status, tags FROM file_records WHERE 1=1 ");
        using var cmd = new SqliteCommand();
        cmd.Connection = connection;

        if (!string.IsNullOrWhiteSpace(query.WorkspaceId))
        {
            sb.Append(" AND workspace_id = @ws_id");
            cmd.Parameters.AddWithValue("@ws_id", query.WorkspaceId);
        }

        if (query.Category != FileCategory.All)
        {
            sb.Append(" AND category = @cat");
            cmd.Parameters.AddWithValue("@cat", (int)query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.Extension))
        {
            sb.Append(" AND extension = @ext");
            cmd.Parameters.AddWithValue("@ext", query.Extension.ToLowerInvariant().TrimStart('.'));
        }

        if (query.OnlyFavorites)
        {
            sb.Append(" AND is_favorite = 1");
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            sb.Append(" AND (name LIKE @term OR absolute_path LIKE @term)");
            cmd.Parameters.AddWithValue("@term", $"%{query.SearchTerm.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            sb.Append(" AND tags LIKE @tag");
            cmd.Parameters.AddWithValue("@tag", $"%{query.Tag.Trim()}%");
        }

        string sortColumn = query.SortBy switch
        {
            "Size" => "size",
            "ModifiedAt" => "modified_at",
            "Category" => "category",
            "Extension" => "extension",
            _ => "name"
        };
        string dir = query.SortDescending ? "DESC" : "ASC";
        sb.Append($" ORDER BY {sortColumn} {dir} LIMIT @limit OFFSET @offset");
        cmd.Parameters.AddWithValue("@limit", query.Limit);
        cmd.Parameters.AddWithValue("@offset", query.Offset);

        cmd.CommandText = sb.ToString();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(MapFileItem(reader));
        }

        return results;
    }

    public async Task<long> GetTotalCountAsync(string? workspaceId = null)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = string.IsNullOrWhiteSpace(workspaceId)
            ? "SELECT COUNT(*) FROM file_records"
            : "SELECT COUNT(*) FROM file_records WHERE workspace_id = @ws_id";

        using var cmd = new SqliteCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            cmd.Parameters.AddWithValue("@ws_id", workspaceId);
        }

        var res = await cmd.ExecuteScalarAsync();
        return res != null ? Convert.ToInt64(res) : 0;
    }

    public async Task<Dictionary<FileCategory, long>> GetCategoryCountsAsync(string? workspaceId = null)
    {
        var counts = new Dictionary<FileCategory, long>();
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = string.IsNullOrWhiteSpace(workspaceId)
            ? "SELECT category, COUNT(*) FROM file_records GROUP BY category"
            : "SELECT category, COUNT(*) FROM file_records WHERE workspace_id = @ws_id GROUP BY category";

        using var cmd = new SqliteCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            cmd.Parameters.AddWithValue("@ws_id", workspaceId);
        }

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var cat = (FileCategory)reader.GetInt32(0);
            long count = reader.GetInt64(1);
            counts[cat] = count;
        }

        return counts;
    }

    public async Task<Dictionary<string, long>> GetExtensionCountsAsync(string? workspaceId = null)
    {
        var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = string.IsNullOrWhiteSpace(workspaceId)
            ? "SELECT extension, COUNT(*) FROM file_records GROUP BY extension ORDER BY COUNT(*) DESC LIMIT 50"
            : "SELECT extension, COUNT(*) FROM file_records WHERE workspace_id = @ws_id GROUP BY extension ORDER BY COUNT(*) DESC LIMIT 50";

        using var cmd = new SqliteCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            cmd.Parameters.AddWithValue("@ws_id", workspaceId);
        }

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string ext = reader.GetString(0);
            long count = reader.GetInt64(1);
            counts[ext] = count;
        }

        return counts;
    }

    public async Task ToggleFavoriteAsync(string fileId, bool isFavorite)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "UPDATE file_records SET is_favorite = @fav WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@fav", isFavorite ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", fileId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateTagsAsync(string fileId, string tags)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "UPDATE file_records SET tags = @tags WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@tags", tags ?? string.Empty);
        cmd.Parameters.AddWithValue("@id", fileId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<FileItem?> GetByIdAsync(string id)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "SELECT id, workspace_id, folder_id, name, extension, category, absolute_path, size, created_at, modified_at, file_hash, is_favorite, thumbnail_status, tags FROM file_records WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapFileItem(reader);
        }

        return null;
    }

    private static FileItem MapFileItem(SqliteDataReader reader)
    {
        return new FileItem
        {
            Id = reader.GetString(0),
            WorkspaceId = reader.GetString(1),
            FolderId = reader.GetString(2),
            Name = reader.GetString(3),
            Extension = reader.GetString(4),
            Category = (FileCategory)reader.GetInt32(5),
            AbsolutePath = reader.GetString(6),
            Size = reader.GetInt64(7),
            CreatedAt = DateTime.Parse(reader.GetString(8)),
            ModifiedAt = DateTime.Parse(reader.GetString(9)),
            FileHash = reader.IsDBNull(10) ? null : reader.GetString(10),
            IsFavorite = reader.GetInt32(11) == 1,
            ThumbnailStatus = reader.GetInt32(12),
            Tags = reader.IsDBNull(13) ? string.Empty : reader.GetString(13)
        };
    }
}
