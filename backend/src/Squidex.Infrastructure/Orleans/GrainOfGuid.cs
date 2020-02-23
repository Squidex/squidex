// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfGuid : GrainBase
    {
        public Guid Key { get; private set; }

        protected GrainOfGuid()
        {
        }

        protected GrainOfGuid(IGrainIdentity identity, IGrainRuntime runtime)
            : base(identity, runtime)
        {
        }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKey());
        }

        public async Task ActivateAsync(Guid key)
        {
            Key = key;

            await OnActivateAsync(key);
        }

        protected virtual Task OnActivateAsync(Guid key)
        {
            return Task.CompletedTask;
        }
    }
}
