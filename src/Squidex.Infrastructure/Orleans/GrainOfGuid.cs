// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfGuid : Grain
    {
        public sealed override Task OnActivateAsync()
        {
            return OnActivateAsync(this.GetPrimaryKey());
        }

        public virtual Task OnActivateAsync(Guid key)
        {
            return TaskHelper.Done;
        }
    }
}
