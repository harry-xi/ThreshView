using System.Threading;
using System.Threading.Tasks;
using ThreshView.Models;

namespace ThreshView.Services;

public interface IImageLoader
{
    Task<ImageDocumentModel> LoadImageAsync(string path, int previewMaxSize = 1024, int thumbnailSize = 128, CancellationToken cancellationToken = default);
}

