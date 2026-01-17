using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ThreshView.Services;

public static class ImageSharpMaskUtils
{
    public static async Task SaveMaskAsync(byte[] grayscaleBuffer, int width, int height, int threshold, bool moreThan,
        Stream output)
    {
        using var maskImg = new Image<L8>(width, height);
        maskImg.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < width; x++)
                {
                    var g = grayscaleBuffer[y * width + x];
                    var mask = moreThan ? g >= threshold : g < threshold;
                    row[x] = new L8(mask ? (byte)255 : (byte)0);
                }
            }
        });
        await maskImg.SaveAsync(output, new PngEncoder());
    }

    public static async Task SaveOverlayAsync(byte[] grayscaleBuffer, byte[] colorBuffer, int width, int height,
        int threshold, bool moreThan, byte overlayR, byte overlayG, byte overlayB, byte overlayA, Stream output)
    {
        using var overlayImg = new Image<Bgra32>(width, height);
        overlayImg.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < width; x++)
                {
                    var idx = (y * width + x) * 4;
                    var b = colorBuffer[idx + 0];
                    var g = colorBuffer[idx + 1];
                    var r = colorBuffer[idx + 2];
                    var a = colorBuffer[idx + 3];
                    var gray = grayscaleBuffer[y * width + x];
                    var mask = moreThan ? gray >= threshold : gray < threshold;
                    if (mask)
                    {
                        var oa = overlayA / 255f;
                        r = (byte)Math.Clamp((int)(overlayR * oa + r * (1 - oa)), 0, 255);
                        g = (byte)Math.Clamp((int)(overlayG * oa + g * (1 - oa)), 0, 255);
                        b = (byte)Math.Clamp((int)(overlayB * oa + b * (1 - oa)), 0, 255);
                        a = 255;
                    }

                    row[x] = new Bgra32(b, g, r, a);
                }
            }
        });
        await overlayImg.SaveAsync(output, new PngEncoder());
    }
}