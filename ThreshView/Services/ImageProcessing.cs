using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ThreshView.Services;

public class ImageProcessing : IImageProcessing
{
    public Task<WriteableBitmap> ThresholdToBitmapAsync(byte[] grayscaleBuffer, int width, int height, int threshold, CancellationToken cancellationToken = default)
    {
        // Create WriteableBitmap in BGRA8888
        var pixelFormat = PixelFormat.Bgra8888;
        var dpi = new Vector(96, 96);
        var wb = new WriteableBitmap(new PixelSize(width, height), dpi, pixelFormat, AlphaFormat.Premul);

        using (var fb = wb.Lock())
        {
            int stride = fb.RowBytes;
            var dest = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Task.FromCanceled<WriteableBitmap>(cancellationToken);

                int srcRow = y * width;
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    byte g = grayscaleBuffer[srcRow + x];
                    byte v = (g >= threshold) ? (byte)255 : (byte)0;
                    int col = rowStart + x * 4;
                    dest[col + 0] = v; // B
                    dest[col + 1] = v; // G
                    dest[col + 2] = v; // R
                    dest[col + 3] = 255; // A
                }
            }

            // Copy to framebuffer
            Marshal.Copy(dest, 0, fb.Address, dest.Length);
        }

        return Task.FromResult(wb);
    }

    public Task<WriteableBitmap> CompositeOverlayAsync(byte[] grayscaleBuffer, byte[] previewColorBuffer, int width, int height, int threshold, byte overlayR, byte overlayG, byte overlayB, byte overlayA, CancellationToken cancellationToken = default)
    {
        var pixelFormat = PixelFormat.Bgra8888;
        var dpi = new Vector(96, 96);
        var wb = new WriteableBitmap(new PixelSize(width, height), dpi, pixelFormat, AlphaFormat.Premul);

        using (var fb = wb.Lock())
        {
            int stride = fb.RowBytes;
            var dest = new byte[stride * height];

            float aF = overlayA / 255f;

            for (int y = 0; y < height; y++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Task.FromCanceled<WriteableBitmap>(cancellationToken);

                int srcRow = y * width;
                int srcColorRow = y * width * 4;
                int rowStart = y * stride;

                for (int x = 0; x < width; x++)
                {
                    byte g = grayscaleBuffer[srcRow + x];
                    int colorIdx = srcColorRow + x * 4;
                    byte origB = previewColorBuffer[colorIdx + 0];
                    byte origG = previewColorBuffer[colorIdx + 1];
                    byte origR = previewColorBuffer[colorIdx + 2];
                    byte origA = previewColorBuffer[colorIdx + 3];

                    byte outR = origR;
                    byte outG = origG;
                    byte outB = origB;
                    byte outA = origA;

                    if (g >= threshold)
                    {
                        // blend overlay color over original: out = overlay * aF + orig * (1-aF)
                        outR = (byte)Math.Clamp((int)(overlayR * aF + origR * (1f - aF)), 0, 255);
                        outG = (byte)Math.Clamp((int)(overlayG * aF + origG * (1f - aF)), 0, 255);
                        outB = (byte)Math.Clamp((int)(overlayB * aF + origB * (1f - aF)), 0, 255);
                        // Keep original alpha or set to 255
                        outA = 255;
                    }

                    int col = rowStart + x * 4;
                    dest[col + 0] = outB;
                    dest[col + 1] = outG;
                    dest[col + 2] = outR;
                    dest[col + 3] = outA;
                }
            }

            Marshal.Copy(dest, 0, fb.Address, dest.Length);
        }

        return Task.FromResult(wb);
    }
}
