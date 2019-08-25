// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Services;

namespace Squidex.Infrastructure.Orleans
{
    public interface IGrainLimiterServiceClient : IGrainServiceClient<IGrainLimiterService>, IGrainLimiterService
    {
    }
}
