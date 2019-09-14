// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettings
    {
        Task<JsonObject> GetAsync(Guid appId, string userId);

        Task SetAsync(Guid appId, string userId, string path, IJsonValue value);

        Task SetAsync(Guid appId, string userId, JsonObject settings);

        Task RemoveAsync(Guid appId, string userId, string path);
    }
}
