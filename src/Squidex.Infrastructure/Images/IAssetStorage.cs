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

namespace Squidex.Infrastructure.Images
{
    public interface IAssetStorage
    {
        Task<Stream> GetAssetAsync(Guid id, string tags = null);

        Task UploadAssetAsync(Guid id, Stream stream, string tags = null);
    }
}