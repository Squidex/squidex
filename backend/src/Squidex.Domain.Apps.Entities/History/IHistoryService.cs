// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History;

public interface IHistoryService
{
    Task<IReadOnlyList<ParsedHistoryEvent>> QueryByChannelAsync(DomainId ownerId, string channelPrefix, int count,
        CancellationToken ct = default);
}
