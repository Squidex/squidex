// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public interface IActivationLimit
    {
        void Register(IActivationLimiter limiter, Grain grain);

        void Unregister(IActivationLimiter limiter, Grain grain);
    }
}
