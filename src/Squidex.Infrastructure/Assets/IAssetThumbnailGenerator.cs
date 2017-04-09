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
        Task<bool> IsValidImageAsync(Stream input);

        Task<Stream> GetThumbnailOrNullAsync(Stream input, int dimension);
    }
}
