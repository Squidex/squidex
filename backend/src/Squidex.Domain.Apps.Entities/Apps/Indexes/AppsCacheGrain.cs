// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    [Reentrant]
    public sealed class AppsCacheGrain : UniqueNameGrain<DomainId>, IAppsCacheGrain
    {
        private readonly IAppRepository appRepository;
        private readonly Dictionary<string, DomainId> appIds = new Dictionary<string, DomainId>();

        public AppsCacheGrain(IAppRepository appRepository)
        {
            this.appRepository = appRepository;
        }

        public async Task<IReadOnlyCollection<DomainId>> GetAppIdsAsync(string[] names)
        {
            var result = new List<DomainId>();

            List<string>? pendingNames = null;

            foreach (var name in names)
            {
                if (!appIds.TryGetValue(name, out var cachedId))
                {
                    pendingNames ??= new List<string>();
                    pendingNames.Add(name);
                }
                else if (cachedId != DomainId.Empty)
                {
                    result.Add(cachedId);
                }
            }

            if (pendingNames != null)
            {
                var foundIds = await appRepository.QueryIdsAsync(pendingNames);

                foreach (var name in pendingNames)
                {
                    if (foundIds.TryGetValue(name, out var id))
                    {
                        appIds[name] = id;

                        result.Add(id);
                    }
                    else
                    {
                        appIds[name] = default;
                    }
                }
            }

            return result;
        }

        public Task AddAsync(DomainId id, string name)
        {
            appIds[name] = id;

            return Task.CompletedTask;
        }

        public Task RemoveAsync(DomainId id)
        {
            var name = appIds.FirstOrDefault(x => x.Value == id).Key;

            if (name != null)
            {
                appIds.Remove(name);
            }

            return Task.CompletedTask;
        }
    }
}
