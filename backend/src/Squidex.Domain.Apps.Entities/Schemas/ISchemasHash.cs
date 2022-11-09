// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Schemas;

public interface ISchemasHash
{
    Task<(Instant Create, string Hash)> GetCurrentHashAsync(IAppEntity app,
        CancellationToken ct = default);

    ValueTask<string> ComputeHashAsync(IAppEntity app, IEnumerable<ISchemaEntity> schemas,
        CancellationToken ct = default);
}
