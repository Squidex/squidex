// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfString : Grain
    {
        public override Task OnActivateAsync()
        {
            return OnActivateAsync(this.GetPrimaryKeyString());
        }

        public virtual Task OnActivateAsync(string key)
        {
            return TaskHelper.Done;
        }
    }
}
