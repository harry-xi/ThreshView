using System;
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
    CancellationTokenSource? _cts;

    public ImageDocumentModel Model { get; }

    public ImageDocumentViewModel(ImageDocumentModel model, IImageProcessing processing)
    {
        Model = model;
        _processing = processing;
        Threshold = 128;
        OverlayR = 255;
        OverlayG = 0;
        OverlayB = 0;
        OverlayA = 128; // semi-transparent
        // produce initial thresholded preview
        _ = RecomputeThresholdAsync();
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

    public int Threshold
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _ = RecomputeThresholdAsync();
        }
    }

    public byte OverlayR
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _ = RecomputeThresholdAsync();
        }
    }

    public byte OverlayG
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _ = RecomputeThresholdAsync();
        }
    }

    public byte OverlayB
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _ = RecomputeThresholdAsync();
        }
    }

    public byte OverlayA
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _ = RecomputeThresholdAsync();
        }
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
                var fb = await _processing.ThresholdToBitmapAsync(Model.GrayscaleBuffer, Model.Width, Model.Height, Threshold, ct).ConfigureAwait(false);
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ThresholdedPreview = fb);
                return;
            }

            var wb = await _processing.CompositeOverlayAsync(Model.GrayscaleBuffer, Model.PreviewColorBuffer, Model.Width, Model.Height, Threshold, OverlayR, OverlayG, OverlayB, OverlayA, ct).ConfigureAwait(false);

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
    }
}
