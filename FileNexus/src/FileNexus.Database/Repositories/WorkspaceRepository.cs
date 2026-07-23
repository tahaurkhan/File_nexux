using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileNexus.Database.Connection;
using FileNexus.Shared.Models;
using Microsoft.Data.Sqlite;

namespace FileNexus.Database.Repositories;

public sealed class WorkspaceRepository : IWorkspaceRepository
{
    private readonly IDatabaseInitializer _dbInitializer;

    public WorkspaceRepository(IDatabaseInitializer dbInitializer)
    {
        _dbInitializer = dbInitializer;
    }

    public async Task<List<Workspace>> GetAllAsync()
    {
        var workspaces = new List<Workspace>();
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "SELECT id, name, description, icon, created_at FROM workspaces ORDER BY created_at ASC";
        using var cmd = new SqliteCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var ws = new Workspace
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Icon = reader.IsDBNull(3) ? "Folder" : reader.GetString(3),
                CreatedAt = DateTime.Parse(reader.GetString(4))
            };
            workspaces.Add(ws);
        }

        foreach (var ws in workspaces)
        {
            ws.Folders = await GetFoldersForWorkspaceAsync(connection, ws.Id);
        }

        return workspaces;
    }

    public async Task<Workspace?> GetByIdAsync(string id)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "SELECT id, name, description, icon, created_at FROM workspaces WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var ws = new Workspace
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Icon = reader.IsDBNull(3) ? "Folder" : reader.GetString(3),
                CreatedAt = DateTime.Parse(reader.GetString(4))
            };
            ws.Folders = await GetFoldersForWorkspaceAsync(connection, ws.Id);
            return ws;
        }

        return null;
    }

    public async Task CreateAsync(Workspace workspace)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "INSERT INTO workspaces (id, name, description, icon, created_at) VALUES (@id, @name, @desc, @icon, @created)";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", workspace.Id);
        cmd.Parameters.AddWithValue("@name", workspace.Name);
        cmd.Parameters.AddWithValue("@desc", workspace.Description);
        cmd.Parameters.AddWithValue("@icon", workspace.Icon);
        cmd.Parameters.AddWithValue("@created", workspace.CreatedAt.ToString("o"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Workspace workspace)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "UPDATE workspaces SET name = @name, description = @desc, icon = @icon WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", workspace.Name);
        cmd.Parameters.AddWithValue("@desc", workspace.Description);
        cmd.Parameters.AddWithValue("@icon", workspace.Icon);
        cmd.Parameters.AddWithValue("@id", workspace.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string id)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "DELETE FROM workspaces WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AddFolderAsync(IndexedFolder folder)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = @"INSERT OR REPLACE INTO workspace_folders (id, workspace_id, path, last_scanned_at, is_active)
                      VALUES (@id, @ws_id, @path, @scanned, @active)";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", folder.Id);
        cmd.Parameters.AddWithValue("@ws_id", folder.WorkspaceId);
        cmd.Parameters.AddWithValue("@path", folder.Path);
        cmd.Parameters.AddWithValue("@scanned", folder.LastScannedAt.HasValue ? folder.LastScannedAt.Value.ToString("o") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@active", folder.IsActive ? 1 : 0);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteFolderAsync(string folderId)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "DELETE FROM workspace_folders WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", folderId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateFolderLastScannedAsync(string folderId, long fileCount)
    {
        using var connection = _dbInitializer.CreateConnection();
        await connection.OpenAsync();

        string sql = "UPDATE workspace_folders SET last_scanned_at = @scanned WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@scanned", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("@id", folderId);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<List<IndexedFolder>> GetFoldersForWorkspaceAsync(SqliteConnection connection, string workspaceId)
    {
        var folders = new List<IndexedFolder>();
        string sql = "SELECT id, workspace_id, path, last_scanned_at, is_active FROM workspace_folders WHERE workspace_id = @ws_id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ws_id", workspaceId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var folder = new IndexedFolder
            {
                Id = reader.GetString(0),
                WorkspaceId = reader.GetString(1),
                Path = reader.GetString(2),
                LastScannedAt = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                IsActive = reader.GetInt32(4) == 1
            };
            folders.Add(folder);
        }

        return folders;
    }
}
