// ==========================================================================
//  GrainV2.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public class GrainV2<TGrainState> : Grain where TGrainState : new()
    {
        private readonly IGrainRuntime runtime;
        private IStorage<TGrainState> storage;

        protected GrainV2(IGrainRuntime runtime)
        {
            this.runtime = runtime;
        }

        protected GrainV2(IGrainIdentity identity, IGrainRuntime runtime, IStorage<TGrainState> storage)
            : base(identity, runtime)
        {
            this.runtime = runtime;
            this.storage = storage;
        }

        protected TGrainState State
        {
            get
            {
                return storage.State;
            }
            set
            {
                storage.State = value;
            }
        }

        protected virtual Task ClearStateAsync()
        {
            return storage.ClearStateAsync();
        }

        protected virtual Task WriteStateAsync()
        {
            return storage.WriteStateAsync();
        }

        protected virtual Task ReadStateAsync()
        {
            return storage.ReadStateAsync();
        }

        public override void Participate(IGrainLifecycle lifecycle)
        {
            base.Participate(lifecycle);

            lifecycle.Subscribe(GrainLifecycleStage.SetupState, OnSetupState);
        }

        private async Task OnSetupState(CancellationToken ct)
        {
            if (!ct.IsCancellationRequested)
            {
                storage = runtime.GetStorage<TGrainState>(this);

                await OnSetupState();
            }
        }

        private async Task OnSetupState()
        {
            await this.ReadStateAsync();
        }
    }
}