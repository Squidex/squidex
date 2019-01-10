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
        public string Key { get; private set; }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKeyString());
        }

        public Task ActivateAsync(string key)
        {
            Key = key;

            return OnActivateAsync(key);
        }

        protected virtual Task OnActivateAsync(string key)
        {
            return TaskHelper.Done;
        }
    }
}
