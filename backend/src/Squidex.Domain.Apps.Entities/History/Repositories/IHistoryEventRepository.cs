// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History.Repositories;

public interface IHistoryEventRepository
{
    Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId appId, string channelPrefix, int count,
        CancellationToken ct = default);

    Task InsertManyAsync(IEnumerable<HistoryEvent> historyEvents,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);
}
