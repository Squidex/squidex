// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Orleans
{
    public interface IActivationLimiter
    {
        void Register(Type grainType, IDeactivater deactivater, int maxActivations);

        void Unregister(Type grainType, IDeactivater deactivater);
    }
}