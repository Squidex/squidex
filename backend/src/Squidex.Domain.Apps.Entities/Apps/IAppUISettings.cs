// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettings
    {
        Task<JsonObject> GetAsync(DomainId appId, string? userId);

        Task SetAsync(DomainId appId, string? userId, string path, IJsonValue value);

        Task SetAsync(DomainId appId, string? userId, JsonObject settings);

        Task RemoveAsync(DomainId appId, string? userId, string path);
    }
}
