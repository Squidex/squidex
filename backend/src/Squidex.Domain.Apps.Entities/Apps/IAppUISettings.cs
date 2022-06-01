﻿// ==========================================================================
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
        Task<JsonValue> GetAsync(DomainId appId, string? userId);

        Task SetAsync(DomainId appId, string? userId, string path, JsonValue value);

        Task SetAsync(DomainId appId, string? userId, JsonValue settings);

        Task RemoveAsync(DomainId appId, string? userId, string path);
    }
}
