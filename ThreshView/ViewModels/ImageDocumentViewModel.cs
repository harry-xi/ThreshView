using System;
using System.ComponentModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ThreshView.Models;
using ThreshView.Services;

namespace ThreshView.ViewModels;

public class ImageDocumentViewModel : ViewModelBase
{
    readonly IImageProcessing _processing;
    readonly MainWindowViewModel _mainWindowViewModel;
    CancellationTokenSource? _cts;

    public ImageDocumentModel Model { get; }

    public ImageDocumentViewModel(ImageDocumentModel model, IImageProcessing processing, MainWindowViewModel mainWindowViewModel)
    {
        Model = model;
        _processing = processing;
        _mainWindowViewModel = mainWindowViewModel;
        _mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        // produce initial thresholded preview
        _ = RecomputeThresholdAsync();
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.Threshold) || e.PropertyName == nameof(MainWindowViewModel.ThresholdColor))
        {
            _ = RecomputeThresholdAsync();
        }
    }

    public string FileName => System.IO.Path.GetFileName(Model.FilePath);

    public Bitmap? Thumbnail => Model.Thumbnail;
    public Bitmap? Preview => Model.Preview;

    private WriteableBitmap? _thresholdedPreview;
    public WriteableBitmap? ThresholdedPreview
    {
        get => _thresholdedPreview;
        private set => this.RaiseAndSetIfChanged(ref _thresholdedPreview, value);
    }

    public async Task RecomputeThresholdAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            if (Model.GrayscaleBuffer == null || Model.GrayscaleBuffer.Length == 0)
                return;

            if (Model.PreviewColorBuffer == null || Model.PreviewColorBuffer.Length == 0)
            {
                // fallback to simple threshold bitmap
                var fb = await _processing.ThresholdToBitmapAsync(Model.GrayscaleBuffer, Model.Width, Model.Height, _mainWindowViewModel.Threshold, ct).ConfigureAwait(false);
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ThresholdedPreview = fb);
                return;
            }

            var color = _mainWindowViewModel.ThresholdColor;
            var wb = await _processing.CompositeOverlayAsync(
                Model.GrayscaleBuffer,
                Model.PreviewColorBuffer,
                Model.Width,
                Model.Height,
                _mainWindowViewModel.Threshold,
                color.R,
                color.G,
                color.B,
                color.A,
                ct).ConfigureAwait(false);

            // marshal to UI thread
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                ThresholdedPreview = wb;
            });
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thresholding failed: {ex}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _thresholdedPreview?.Dispose();
        _mainWindowViewModel.PropertyChanged -= MainWindowViewModel_PropertyChanged;
    }
}
