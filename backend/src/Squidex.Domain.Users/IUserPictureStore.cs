// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Users;

public interface IUserPictureStore
{
    Task UploadAsync(string userId, Stream stream,
        CancellationToken ct = default);

    Task DownloadAsync(string userId, Stream stream,
        CancellationToken ct = default);
}
