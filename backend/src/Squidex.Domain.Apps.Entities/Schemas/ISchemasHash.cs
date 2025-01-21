// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Schemas;

public interface ISchemasHash
{
    Task<SchemasHashKey> GetCurrentHashAsync(App app,
        CancellationToken ct = default);
}
