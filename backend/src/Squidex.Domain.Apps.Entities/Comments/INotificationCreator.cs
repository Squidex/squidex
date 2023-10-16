// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Comments;

public interface INotificationPublisher
{
    Task NotifyAsync(string userId, string text, Uri? url,
        CancellationToken ct = default);
}
