// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public interface IActivationLimiter
    {
        void Register<T>(Guid id, int limit) where T : IGrainWithGuidKey, IDeactivatableGrain;

        void Register<T>(string id, int limit) where T : IGrainWithStringKey, IDeactivatableGrain;

        void Unregister<T>(Guid id) where T : IGrainWithGuidKey, IDeactivatableGrain;

        void Unregister<T>(string id) where T : IGrainWithStringKey, IDeactivatableGrain;
    }
}