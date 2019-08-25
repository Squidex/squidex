// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans.Runtime.Services;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class GrainLimiterServiceClient : GrainServiceClient<IGrainLimiterService>, IGrainLimiterServiceClient
    {
        public GrainLimiterServiceClient(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Task RegisterAsync(Type type, Guid id, int limit)
        {
            return GrainService.RegisterAsync(type, id, limit);
        }

        public Task RegisterAsync(Type type, string id, int limit)
        {
            return GrainService.RegisterAsync(type, id, limit);
        }

        public Task UnregisterAsync(Type type, Guid id)
        {
            return GrainService.UnregisterAsync(type, id);
        }

        public Task UnregisterAsync(Type type, string id)
        {
            return GrainService.UnregisterAsync(type, id);
        }
    }
}
