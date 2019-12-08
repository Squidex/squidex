// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppImageStore
    {
        Task UploadAsync(Guid appId, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(Guid appId, Stream stream, CancellationToken ct = default);
    }
}
