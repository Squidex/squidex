// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public interface ITextIndexerState
{
    Task<Dictionary<DomainId, TextContentState>> GetAsync(HashSet<DomainId> ids,
        CancellationToken ct = default);

    Task SetAsync(List<TextContentState> updates,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);
}
