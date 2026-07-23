# FileNexus — System Architecture & Technical Specification

## 1. System Architecture Overview

FileNexus follows **Clean Architecture** and **MVVM (Model-View-ViewModel)** principles, strictly separating concerns into decoupled, modular projects.

```mermaid
graph TD
    subgraph Presentation Layer [FileNexus.UI - Avalonia UI / MVVM]
        Views[Views / Controls]
        VMs[ViewModels]
        Nav[Navigation Service]
    end

    subgraph Business Core [FileNexus.Core]
        WorkServices[Workspace Service]
        FileServices[File Query & Search Service]
        ScanOrch[Scanner Orchestrator]
        CategoryEngine[Extension & Category Classifier]
    end

    subgraph Data & Storage [FileNexus.Database]
        SqliteConn[SQLite Connection Manager (WAL)]
        WorkRepo[Workspace Repository]
        FileRepo[File Record Repository]
    end

    subgraph Native Interop [FileNexus.Interop & Rust filenexus_engine]
        FFIBridge[P/Invoke Bridge]
        RustEngine[Rust Multi-Threaded File Scanner Engine]
    end

    subgraph Extensibility [FileNexus.Plugins]
        PluginHost[Plugin Contract & Manager]
    end

    subgraph Shared Contracts [FileNexus.Shared]
        Models[Domain Models & DTOs]
        Enums[Enums & Constants]
    end

    Views --> VMs
    VMs --> WorkServices
    VMs --> FileServices
    WorkServices --> WorkRepo
    FileServices --> FileRepo
    ScanOrch --> FFIBridge
    FFIBridge --> RustEngine
    WorkServices --> PluginHost
    WorkRepo --> SqliteConn
    FileRepo --> SqliteConn
    FileServices --> Models
    WorkRepo --> Models
    FileRepo --> Models
```

---

## 2. Project Responsibilities & Dependency Hierarchy

| Project Name | Primary Responsibility | Dependencies |
| :--- | :--- | :--- |
| **`FileNexus.Shared`** | Core Domain Models, DTOs, Enums, Interfaces, System Constants. | None |
| **`FileNexus.Interop`** | Native P/Invoke interop bindings to `filenexus_engine` Rust shared library. | `FileNexus.Shared` |
| **`FileNexus.Database`** | SQLite WAL-mode initialization, schema migrations, and high-performance repositories. | `FileNexus.Shared`, `Microsoft.Data.Sqlite` |
| **`FileNexus.Plugins`** | Extensibility interfaces and plugin discovery contracts. | `FileNexus.Shared` |
| **`FileNexus.Core`** | Application workflows, scanning orchestration, category indexing, and search engine. | `FileNexus.Shared`, `FileNexus.Database`, `FileNexus.Interop`, `FileNexus.Plugins`, `Microsoft.Extensions.DependencyInjection` |
| **`FileNexus.UI`** | Avalonia UI responsive interface, themes, viewmodels, virtualized lists, dialogs. | `FileNexus.Shared`, `FileNexus.Core`, `CommunityToolkit.Mvvm` |
| **`native/filenexus_engine`** | High-speed, parallel native Rust scanner for filesystem traversal and file metadata extraction. | Rust std, Rayon/Walkdir/SIMD |

---

## 3. Database Schema Specification (SQLite - WAL Mode)

```sql
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
```
