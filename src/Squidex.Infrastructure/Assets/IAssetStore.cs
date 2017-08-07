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
        string GenerateSourceUrl(string id, long version, string suffix);

        Task CopyTemporaryAsync(string name, string id, long version, string suffix);

        Task DownloadAsync(string id, long version, string suffix, Stream stream);

        Task UploadTemporaryAsync(string name, Stream stream);

        Task UploadAsync(string id, long version, string suffix, Stream stream);

        Task DeleteTemporaryAsync(string name);
    }
}