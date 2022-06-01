// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettings
    {
        Task<JsonValue2> GetAsync(DomainId appId, string? userId);

        Task SetAsync(DomainId appId, string? userId, string path, JsonValue2 value);

        Task SetAsync(DomainId appId, string? userId, JsonValue2 settings);

        Task RemoveAsync(DomainId appId, string? userId, string path);
    }
}
