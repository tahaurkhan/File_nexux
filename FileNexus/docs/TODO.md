# FileNexus — Active Task Board & Sprint Checklist

> **Current Focus:** Phase 1 (UI Shell & Core Setup) & Phase 2 (Rust FFI Infrastructure)  
> **Rule:** Check off items as they are implemented and verified with tests.

---

## 🏃 Active Sprint Tasks

### 1. Presentation & UI Shell (`FileNexus.UI`)
- [ ] Implement main window layout with collapsible left sidebar navigation
- [ ] Configure Avalonia UI dark/light theme resource dictionaries
- [ ] Integrate `CommunityToolkit.Mvvm` source generators into ViewModels
- [ ] Build `SplashWindow` and `WorkspaceWizardView` for first-run onboarding
- [ ] Create basic `DashboardView` with placeholder metric widgets

### 2. Core Services & Infrastructure (`FileNexus.Core`)
- [ ] Set up Dependency Injection container builder in `App.axaml.cs`
- [ ] Define core interfaces:
  - [ ] `IWorkspaceService`
  - [ ] `ISearchEngineService`
  - [ ] `IDatabaseContext`
  - [ ] `INativeScannerBridge`
- [ ] Create `AppSettings` model with JSON serialization/deserialization logic
- [ ] Implement SQLite connection manager with WAL mode initialization

### 3. Native Engine & Interop (`filenexus_engine` & `FileNexus.Native`)
- [ ] Create `filenexus_engine` Rust crate with `Cargo.toml` (`crate-type = ["cdylib"]`)
- [ ] Implement Rust C-ABI exported functions:
  - [ ] `filenexus_scan_directory(path, callback)`
  - [ ] `filenexus_free_string(ptr)`
- [ ] Write `[LibraryImport]` P/Invoke bindings in `FileNexus.Native`
- [ ] Create basic C# integration test validating string passing between C# and Rust

---

## 📋 Backlog & Upcoming Tasks

### Database Pipeline
- [ ] Create SQLite migration scripts for `workspaces` and `file_records` tables
- [ ] Write bulk batch-insert routine for streaming scan results into SQLite
- [ ] Implement database benchmark tests verifying 100k records write under 2 seconds

### Explorers & Search
- [ ] Implement `CategoryExplorerViewModel` (Audio, Video, Code, Docs, Archives)
- [ ] Implement `ExtensionExplorerViewModel` with file count aggregations
- [ ] Build virtualized `BookCardView` control for PDF/EPUB browsing
- [ ] Implement async fuzzy search query parser

### System & Watcher
- [ ] Implement cross-platform OS trash bin file deletion helper
- [ ] Set up `System.Reactive` debounce buffer for filesystem event monitoring

---

## 🐞 Bug Tracker & Refactoring
*(No active bugs reported for pre-release build)*

---

<p align="center">
  <em>FileNexus Sprint Task Checklist — Updated for Phase 1/2 Execution.</em>
</p>