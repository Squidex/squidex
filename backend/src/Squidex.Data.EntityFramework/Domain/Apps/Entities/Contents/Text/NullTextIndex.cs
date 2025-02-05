// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class NullTextIndex : ITextIndex
{
    public Task ClearAsync(
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        return Task.FromResult<List<DomainId>?>(null);
    }

    public Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        return Task.FromResult<List<DomainId>?>(null);
    }
}
