// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettingsGrain : IGrainWithStringKey
    {
        Task<J<JsonValue2>> GetAsync();

        Task SetAsync(string path, J<JsonValue2> value);

        Task SetAsync(J<JsonValue2> settings);

        Task RemoveAsync(string path);

        Task ClearAsync();
    }
}
