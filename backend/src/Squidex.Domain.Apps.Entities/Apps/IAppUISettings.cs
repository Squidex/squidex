// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps;

public interface IAppUISettings
{
    Task<JsonObject> GetAsync(DomainId appId, string? userId,
        CancellationToken ct = default);

    Task SetAsync(DomainId appId, string? userId, string path, JsonValue value,
        CancellationToken ct = default);

    Task SetAsync(DomainId appId, string? userId, JsonObject settings,
        CancellationToken ct = default);

    Task RemoveAsync(DomainId appId, string? userId, string path,
        CancellationToken ct = default);
}
