# FileNexus — Project Roadmap & Milestone Specifications

> **Current Version:** `0.1.0-alpha`  
> **Status:** Active Phase 1 Implementation  
> **Target Release:** Cross-Platform Native Engine & UI Shell  

---

## 🧭 Milestone Overview

```mermaid
gantt
    title FileNexus Strategic Release Timeline
    dateFormat  YYYY-MM
    section Phase 1: Core Foundation
    Architecture & Core Docs      :done, p1_1, 2026-05, 2026-06
    UI Shell & DI Setup           :active, p1_2, 2026-06, 2026-07
    section Phase 2: Engine & DB
    Rust Scanner FFI Bridge       :p2_1, 2026-07, 2026-08
    SQLite Indexing Pipeline      :p2_2, 2026-08, 2026-09
    section Phase 3: Explorers
    Category & Extension Views    :p3_1, 2026-09, 2026-10
    Smart Search & Previewers     :p3_2, 2026-10, 2026-11
    section Phase 4: Polish & Plugins
    Plugin SDK & Realtime Watcher :p4_1, 2026-11, 2026-12
    v1.0 Production Release       :milestone, m1, 2026-12, 2026-12