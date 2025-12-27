using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ThreshView.Models;

namespace ThreshView.Services;

public class ImageLoader : IImageLoader
{
    public async Task<ImageDocumentModel> LoadImageAsync(string path, int previewMaxSize = 1024,
        int thumbnailSize = 128, CancellationToken cancellationToken = default)
    {
        // Load using ImageSharp to a known pixel format
        using var fs = File.OpenRead(path);
        var img = await Image.LoadAsync<Rgba32>(fs, cancellationToken).ConfigureAwait(false);

        var width = img.Width;
        var height = img.Height;

        // Create preview (scaled) and thumbnail (scaled)
        var preview = img.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(previewMaxSize, previewMaxSize)
        }));

        var thumb = img.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(thumbnailSize, thumbnailSize)
        }));

        // Convert preview to grayscale buffer and color buffer (BGRA order for Avalonia)
        var pW = preview.Width;
        var pH = preview.Height;
        var gray = new byte[pW * pH];
        var color = new byte[pW * pH * 4];
        preview.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var p = row[x];
                    // luminance
                    var l = 0.2126f * p.R + 0.7152f * p.G + 0.0722f * p.B;
                    gray[y * pW + x] = (byte)Math.Clamp((int)l, 0, 255);

                    var idx = (y * pW + x) * 4;
                    color[idx + 0] = p.B; // B
                    color[idx + 1] = p.G; // G
                    color[idx + 2] = p.R; // R
                    color[idx + 3] = p.A; // A
                }
            }
        });

        // Convert ImageSharp images to Avalonia Bitmaps via memory stream
        Bitmap previewBitmap;
        Bitmap thumbBitmap;
        using (var ms = new MemoryStream())
        {
            await preview.SaveAsPngAsync(ms, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            previewBitmap = new Bitmap(ms);
        }

        using (var ms = new MemoryStream())
        {
            await thumb.SaveAsPngAsync(ms, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            thumbBitmap = new Bitmap(ms);
        }

        return new ImageDocumentModel
        {
            FilePath = path,
            Width = pW,
            Height = pH,
            GrayscaleBuffer = gray,
            PreviewColorBuffer = color,
            Preview = previewBitmap,
            Thumbnail = thumbBitmap,
            LoadedAt = DateTime.UtcNow
        };
    }
}