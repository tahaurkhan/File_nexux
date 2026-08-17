using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FileNexus.Shared.Models;
using FileNexus.UI.Services;

namespace FileNexus.UI.ViewModels;

public partial class FileItemViewModel : ViewModelBase
{
    public FileItem Item { get; }

    [ObservableProperty]
    public partial Bitmap? Thumbnail { get; set; }

    public string Id => Item.Id;
    public string Name => Item.Name;
    public string Extension => Item.Extension;
    public string UpperExtension => Item.UpperExtension;
    public string AbsolutePath => Item.AbsolutePath;
    public long Size => Item.Size;
    public string FormattedSize => Item.FormattedSize;
    public string Icon => Item.Icon;
    public string BadgeColor => Item.BadgeColor;
    public string Category => Item.Category.ToString();
    public System.DateTime ModifiedAt => Item.ModifiedAt;
    public System.DateTime CreatedAt => Item.CreatedAt;
    public bool IsImage => Item.IsImage;

    public string Tags
    {
        get => Item.Tags;
        set
        {
            Item.Tags = value;
            OnPropertyChanged(nameof(Tags));
        }
    }

    public bool IsFavorite
    {
        get => Item.IsFavorite;
        set
        {
            Item.IsFavorite = value;
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    public FileItemViewModel(FileItem item)
    {
        Item = item;
        Thumbnail = ThumbnailService.GetCachedThumbnail(item.AbsolutePath);
        if (Thumbnail == null && item.IsImage)
        {
            _ = LoadThumbnailAsync();
        }
    }

    public async Task LoadThumbnailAsync()
    {
        if (Item.IsImage && !string.IsNullOrEmpty(Item.AbsolutePath) && File.Exists(Item.AbsolutePath))
        {
            var bitmap = await ThumbnailService.LoadThumbnailAsync(Item.AbsolutePath);
            if (bitmap != null)
            {
                Thumbnail = bitmap;
            }
        }
    }
}
