// ==========================================================================
//  IAssetStorage.cs
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
        Task<Stream> GetAssetAsync(string name);

        Task UploadAssetAsync(string name, Stream stream);
    }
}