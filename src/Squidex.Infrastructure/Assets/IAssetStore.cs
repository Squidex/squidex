// ==========================================================================
//  IAssetStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetStore
    {
        Task<Stream> GetAssetAsync(Guid id, long version, string suffix = null);

        Task UploadAssetAsync(Guid id, long version, Stream stream, string suffix = null);
    }
}