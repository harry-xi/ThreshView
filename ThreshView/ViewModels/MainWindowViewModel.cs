using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ThreshView.Services;

namespace ThreshView.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IImageLoader _loader;
    private readonly IImageProcessing _processing;

    public MainWindowViewModel() : this(new ImageLoader(), new ImageProcessing())
    {
    }

    private MainWindowViewModel(IImageLoader loader, IImageProcessing processing)
    {
        _loader = loader;
        _processing = processing;
        OpenImages = new ObservableCollection<ImageDocumentViewModel>();
        OpenCommand = ReactiveCommand.CreateFromTask(OpenFilesAsync);
        SaveMask = ReactiveCommand.CreateFromTask(SaveMaskAsync);
        SaveOverlay = ReactiveCommand.CreateFromTask(SaveOverlayAsync);
    }

    public ObservableCollection<ImageDocumentViewModel> OpenImages { get; }

    public ImageDocumentViewModel? SelectedImage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Color ThresholdColor
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = new(100, 255, 0, 0);

    public int Threshold
    {
        get;
        set =>
            this.RaiseAndSetIfChanged(ref field, value);
    } = 100;

    public bool ThresholdMoreThen
    {
        get;
        set =>
            this.RaiseAndSetIfChanged(ref field, value);
    } = true;


    public ICommand OpenCommand { get; }

    public ICommand SaveMask { get; }

    public ICommand SaveOverlay { get; }

    public IColorPalette ColorPalette { get; } = new ThresholdColorPaletteIColorPalette();

    private static IStorageProvider? GetStorageProvider()
    {
        var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (topLevel == null) return null;
        var storageProvider = TopLevel.GetTopLevel(topLevel)?.StorageProvider;
        return storageProvider;
    }


    private async Task OpenFilesAsync()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return;
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true
        });
        if (files.Count == 0) return;

        foreach (var file in files)
        {
            var path = file.Path.LocalPath;
            var model = await _loader.LoadImageAsync(path);
            var vm = new ImageDocumentViewModel(model, _processing, this);
            OpenImages.Add(vm);
            SelectedImage = vm;
        }
    }

    private async Task SaveMaskAsync()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return;
        if (SelectedImage?.Model == null) return;
        var model = SelectedImage.Model;
        if (model.GrayscaleBuffer.Length == 0) return;
        using var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices =
            [
                new FilePickerFileType("PNG") { Patterns = ["*.png"] }
            ]
        });
        if (file == null) return;
        await using var stream = await file.OpenWriteAsync();
        await ImageSharpMaskUtils.SaveMaskAsync(
            model.GrayscaleBuffer,
            model.Width,
            model.Height,
            Threshold,
            ThresholdMoreThen,
            stream
        );
    }

    private async Task SaveOverlayAsync()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return;
        if (SelectedImage?.Model == null) return;
        var model = SelectedImage.Model;
        if (model.GrayscaleBuffer.Length == 0 || model.PreviewColorBuffer.Length == 0) return;
        using var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices =
            [
                new FilePickerFileType("PNG") { Patterns = ["*.png"] }
            ]
        });
        if (file == null) return;
        var color = ThresholdColor;
        await using var stream = await file.OpenWriteAsync();
        await ImageSharpMaskUtils.SaveOverlayAsync(
            model.GrayscaleBuffer,
            model.PreviewColorBuffer,
            model.Width,
            model.Height,
            Threshold,
            ThresholdMoreThen,
            color.R, color.G, color.B, color.A,
            stream
        );
    }
}