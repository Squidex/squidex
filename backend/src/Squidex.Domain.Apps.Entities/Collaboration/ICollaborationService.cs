// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Collaboration;

public interface ICollaborationService
{
    Task NotifyAsync(string userId, string text, RefToken actor, Uri? url, bool skipHandlers,
        CancellationToken ct = default);

    Task CommentAsync(NamedId<DomainId> appId, DomainId resourceId, string text, RefToken actor, Uri? url, bool skipHandlers,
        CancellationToken ct = default);

    string UserDocument(string userId);

    string ResourceDocument(NamedId<DomainId> appId, DomainId resourceId);
}
