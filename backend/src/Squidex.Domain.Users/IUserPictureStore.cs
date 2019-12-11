﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Domain.Users
{
    public interface IUserPictureStore
    {
        Task UploadAsync(string userId, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(string userId, Stream stream, CancellationToken ct = default);
    }
}
