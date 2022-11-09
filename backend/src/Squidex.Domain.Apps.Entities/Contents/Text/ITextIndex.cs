// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public interface ITextIndex
{
    Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
        CancellationToken ct = default);

    Task<List<DomainId>?> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);

    Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default);
}
