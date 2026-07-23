namespace FileNexus.Shared.Enums;

/// <summary>
/// Virtual file category classifications for grouping files independent of physical folder structure.
/// </summary>
public enum FileCategory
{
    All = 0,
    Documents = 1,
    Images = 2,
    Videos = 3,
    Audio = 4,
    Code = 5,
    Books = 6,
    Archives = 7,
    Executables = 8,
    Other = 99
}
