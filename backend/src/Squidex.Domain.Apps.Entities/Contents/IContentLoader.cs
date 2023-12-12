// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IContentLoader
{
    Task<Content?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default);
}
