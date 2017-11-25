// ==========================================================================
//  StatefulActor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public abstract class StatefulObject<T> : DisposableObjectBase
    {
        private IStateHolder<T> stateHolder;

        public T State
        {
            get
            {
                if (stateHolder != null)
                {
                    return stateHolder.State;
                }
                else
                {
                    return default(T);
                }
            }

            protected set
            {
                if (stateHolder != null)
                {
                    stateHolder.State = value;
                }
            }
        }

        public Task ActivateAsync(IStateHolder<T> stateHolder)
        {
            Guard.NotNull(stateHolder, nameof(stateHolder));

            this.stateHolder = stateHolder;

            return stateHolder.ReadAsync();
        }

        public virtual async Task ReadStateAsync()
        {
            if (stateHolder != null)
            {
                await stateHolder.ReadAsync();
            }
        }

        public virtual async Task WriteStateAsync()
        {
            if (stateHolder != null)
            {
                await stateHolder.WriteAsync();
            }
        }

        protected override void DisposeObject(bool disposing)
        {
        }
    }
}
