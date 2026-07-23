using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace FileNexus.Database.Connection;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    public string DbPath { get; }

    public DatabaseInitializer(string? customDbPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customDbPath))
        {
            DbPath = customDbPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "FileNexus");
            Directory.CreateDirectory(dir);
            DbPath = Path.Combine(dir, "filenexus.db");
        }
    }

    public SqliteConnection CreateConnection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        };
        return new SqliteConnection(builder.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        // Enable Write-Ahead Logging (WAL) and performance optimizations
        using (var pragmaCmd = connection.CreateCommand())
        {
            pragmaCmd.CommandText = @"
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;
                PRAGMA temp_store = MEMORY;
                PRAGMA foreign_keys = ON;
            ";
            await pragmaCmd.ExecuteNonQueryAsync();
        }

        // Migration Schema Creation
        using (var schemaCmd = connection.CreateCommand())
        {
            schemaCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS workspaces (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    description TEXT,
                    icon TEXT,
                    created_at TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS workspace_folders (
                    id TEXT PRIMARY KEY,
                    workspace_id TEXT NOT NULL,
                    path TEXT NOT NULL UNIQUE,
                    last_scanned_at TEXT,
                    is_active INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY(workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS file_records (
                    id TEXT PRIMARY KEY,
                    workspace_id TEXT NOT NULL,
                    folder_id TEXT NOT NULL,
                    name TEXT NOT NULL,
                    extension TEXT NOT NULL,
                    category INTEGER NOT NULL,
                    absolute_path TEXT NOT NULL UNIQUE,
                    size INTEGER NOT NULL,
                    created_at TEXT NOT NULL,
                    modified_at TEXT NOT NULL,
                    file_hash TEXT,
                    is_favorite INTEGER NOT NULL DEFAULT 0,
                    thumbnail_status INTEGER NOT NULL DEFAULT 0,
                    tags TEXT,
                    FOREIGN KEY(workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE,
                    FOREIGN KEY(folder_id) REFERENCES workspace_folders(id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_file_records_category ON file_records(category);
                CREATE INDEX IF NOT EXISTS idx_file_records_extension ON file_records(extension);
                CREATE INDEX IF NOT EXISTS idx_file_records_workspace ON file_records(workspace_id);
                CREATE INDEX IF NOT EXISTS idx_file_records_favorite ON file_records(is_favorite);
                CREATE INDEX IF NOT EXISTS idx_file_records_path ON file_records(absolute_path);
            ";
            await schemaCmd.ExecuteNonQueryAsync();
        }
    }
}
