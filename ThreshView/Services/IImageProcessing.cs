using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace ThreshView.Services;

public interface IImageProcessing
{
    Task<WriteableBitmap> ThresholdToBitmapAsync(byte[] grayscaleBuffer, int width, int height, int threshold,
        CancellationToken cancellationToken = default);

    // Produce a composited overlay: for pixels >= threshold, blend overlayColor (BGRA) with previewColorBuffer using alpha.
    Task<WriteableBitmap> CompositeOverlayAsync(byte[] grayscaleBuffer, byte[] previewColorBuffer, int width,
        int height, int threshold, bool moreThan, byte overlayR, byte overlayG, byte overlayB, byte overlayA,
        CancellationToken cancellationToken = default);
}