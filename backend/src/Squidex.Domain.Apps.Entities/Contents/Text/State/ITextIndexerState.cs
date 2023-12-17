// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public interface ITextIndexerState
{
    Task<Dictionary<UniqueContentId, TextContentState>> GetAsync(HashSet<UniqueContentId> ids,
        CancellationToken ct = default);

    Task SetAsync(List<TextContentState> updates,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);
}
