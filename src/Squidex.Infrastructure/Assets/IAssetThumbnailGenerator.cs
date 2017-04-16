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
        Task<ImageInfo> GetImageInfoAsync(Stream source);

        Task CreateThumbnailAsync(Stream source, Stream destination, int? width, int? height, string mode);
    }
}
