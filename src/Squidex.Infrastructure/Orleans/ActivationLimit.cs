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
    public static class ActivationLimit
    {
        private sealed class GuidActivationLimit<T> : IActivationLimit where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            private readonly int limit;

            public GuidActivationLimit(int limit)
            {
                this.limit = limit;
            }

            public void Register(IActivationLimiter limiter, Grain grain)
            {
                if (limiter != null)
                {
                    limiter.Register<T>(GetKey(grain), limit);
                }
            }

            public void Unregister(IActivationLimiter limiter, Grain grain)
            {
                if (limiter != null)
                {
                    limiter.Unregister<T>(GetKey(grain));
                }
            }

            private Guid GetKey(Grain grain)
            {
                if (grain is GrainOfGuid guidGrain)
                {
                    return guidGrain.Key;
                }

                return grain.GetPrimaryKey();
            }
        }

        private sealed class StringActivationLimit<T> : IActivationLimit where T : IGrainWithStringKey, IDeactivatableGrain
        {
            private readonly int limit;

            public StringActivationLimit(int limit)
            {
                this.limit = limit;
            }

            public void Register(IActivationLimiter limiter, Grain grain)
            {
                if (limiter != null)
                {
                    limiter.Register<T>(GetKey(grain), limit);
                }
            }

            public void Unregister(IActivationLimiter limiter, Grain grain)
            {
                if (limiter != null)
                {
                    limiter.Unregister<T>(GetKey(grain));
                }
            }

            private string GetKey(Grain grain)
            {
                if (grain is GrainOfString stringGrain)
                {
                    return stringGrain.Key;
                }

                return grain.GetPrimaryKeyString();
            }
        }

        public static IActivationLimit ForGuidKey<T>(int limit) where T : IGrainWithGuidKey, IDeactivatableGrain
        {
            return new GuidActivationLimit<T>(limit);
        }

        public static IActivationLimit ForStringKey<T>(int limit) where T : IGrainWithStringKey, IDeactivatableGrain
        {
            return new StringActivationLimit<T>(limit);
        }
    }
}