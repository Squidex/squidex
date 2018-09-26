// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppUISettingsGrain : IGrainWithGuidKey
    {
        Task<J<JObject>> GetAsync();

        Task SetAsync(string path, J<JToken> value);

        Task SetAsync(J<JObject> settings);

        Task RemoveAsync(string path);
    }
}
