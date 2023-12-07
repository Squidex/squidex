// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas;

public interface ISchemasHash
{
    Task<(Instant Create, string Hash)> GetCurrentHashAsync(App app,
        CancellationToken ct = default);

    ValueTask<string> ComputeHashAsync(App app, IEnumerable<Schema> schemas,
        CancellationToken ct = default);
}
