// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans.Services;

namespace Squidex.Infrastructure.Orleans
{
    public interface IGrainLimiterService : IGrainService
    {
        Task RegisterAsync(Type type, Guid id, int limit);

        Task RegisterAsync(Type type, string id, int limit);

        Task UnregisterAsync(Type type, Guid id);

        Task UnregisterAsync(Type type, string id);
    }
}