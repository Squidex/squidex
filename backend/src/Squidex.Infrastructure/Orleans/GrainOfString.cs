// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfString : GrainBase
    {
        public string Key { get; private set; }

        protected GrainOfString()
        {
        }

        protected GrainOfString(IGrainIdentity identity, IGrainRuntime runtime)
            : base(identity, runtime)
        {
        }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKeyString());
        }

        public async Task ActivateAsync(string key)
        {
            Key = key;

            await OnLoadAsync(key);
            await OnActivateAsync(key);
        }

        protected virtual Task OnLoadAsync(string key)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnActivateAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}
