// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppImageStore
    {
        Task UploadAsync(DomainId appId, Stream stream,
            CancellationToken ct = default);

        Task DownloadAsync(DomainId appId, Stream stream,
            CancellationToken ct = default);
    }
}
