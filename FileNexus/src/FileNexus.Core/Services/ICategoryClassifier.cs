using FileNexus.Shared.Enums;

namespace FileNexus.Core.Services;

public interface ICategoryClassifier
{
    FileCategory Classify(string extension);
}
