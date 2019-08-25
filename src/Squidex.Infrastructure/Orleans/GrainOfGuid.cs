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
    public abstract class GrainOfGuid : GrainBase
    {
        public Guid Key { get; private set; }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKey());
        }

        public async Task ActivateAsync(Guid key)
        {
            Key = key;

            await OnLoadAsync(key);
            await OnActivateAsync(key);
        }

        protected virtual Task OnLoadAsync(Guid key)
        {
            return TaskHelper.Done;
        }

        protected virtual Task OnActivateAsync(Guid key)
        {
            return TaskHelper.Done;
        }
    }
}
