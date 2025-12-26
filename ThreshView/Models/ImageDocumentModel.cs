using System;
using Avalonia.Media.Imaging;

namespace ThreshView.Models;

public class ImageDocumentModel
{
    public string FilePath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] GrayscaleBuffer { get; set; } = Array.Empty<byte>();
    // Preview color buffer stored as BGRA bytes (stride = Width * 4)
    public byte[] PreviewColorBuffer { get; set; } = Array.Empty<byte>();
    public Bitmap? Thumbnail { get; set; }
    public Bitmap? Preview { get; set; }
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}
