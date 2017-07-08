// ==========================================================================
//  IAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetStore
    {
        Task DownloadAsync(string id, long version, string suffix, Stream stream);

        Task UploadAsync(string id, long version, string suffix, Stream stream);
    }
}