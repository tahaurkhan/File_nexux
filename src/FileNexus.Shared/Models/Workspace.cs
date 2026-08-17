using System;
using System.Collections.Generic;

namespace FileNexus.Shared.Models;

/// <summary>
/// Represents a user workspace containing a collection of indexed folders.
/// </summary>
public sealed class Workspace
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "Folder";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<IndexedFolder> Folders { get; set; } = new();
}
