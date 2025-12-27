using System.Collections.ObjectModel;

using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using ThreshView.Services;


namespace ThreshView.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IImageLoader _loader;
    private readonly IImageProcessing _processing;

    public MainWindowViewModel() : this(new ImageLoader(), new ImageProcessing())
    {
    }

    public MainWindowViewModel(IImageLoader loader, IImageProcessing processing)
    {
        _loader = loader;
        _processing = processing;
        OpenImages = new ObservableCollection<ImageDocumentViewModel>();
        OpenCommand = ReactiveCommand.CreateFromTask(OpenFilesAsync);
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
    // Expose as ICommand so XAML analyzers recognize it
    public ICommand OpenCommand { get; }

    private async Task OpenFilesAsync()
    {
        var dlg = new Avalonia.Controls.OpenFileDialog();
        dlg.AllowMultiple = true;
        var res = await dlg.ShowAsync(Avalonia.Application.Current!.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
        if (res == null || res.Length == 0) return;

        foreach (var path in res)
        {
            var model = await _loader.LoadImageAsync(path);
            var vm = new ImageDocumentViewModel(model, _processing, this);
            OpenImages.Add(vm);
            SelectedImage = vm;
        }
    }
}
