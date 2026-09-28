// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public interface ITextIndexRebuilder
{
    Task RebuildAsync(DomainId appId, IReadOnlyCollection<DomainId> contentIds,
        CancellationToken ct = default);
}
