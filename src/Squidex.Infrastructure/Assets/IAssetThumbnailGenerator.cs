// ==========================================================================
//  IAssetThumbnailGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetThumbnailGenerator
    {
        Task<ImageInfo> GetImageInfoAsync(Stream input);

        Task<Stream> CreateThumbnailAsync(Stream input, int? width, int? height, string mode);
    }
}
