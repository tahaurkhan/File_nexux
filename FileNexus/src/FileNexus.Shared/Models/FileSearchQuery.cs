using System;
using System.Collections.Generic;
using FileNexus.Shared.Enums;

namespace FileNexus.Shared.Models;

/// <summary>
/// Parameters for querying, searching, and filtering virtual file records.
/// </summary>
public sealed class FileSearchQuery
{
    public string? SearchTerm { get; set; }
    public string? WorkspaceId { get; set; }
    public FileCategory Category { get; set; } = FileCategory.All;
    public string? Extension { get; set; }
    public bool OnlyFavorites { get; set; }
    public string? Tag { get; set; }
    public string SortBy { get; set; } = "Name"; // Name, Size, ModifiedAt, Category
    public bool SortDescending { get; set; } = false;
    public int Limit { get; set; } = 500;
    public int Offset { get; set; } = 0;
}
