// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrain<T, TState> : GrainOfString where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
    {
        private readonly T domainObject;

        public TState Snapshot
        {
            get { return domainObject.Snapshot; }
        }

        protected T DomainObject
        {
            get { return domainObject; }
        }

        protected DomainObjectGrain(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            domainObject = serviceProvider.GetRequiredService<T>();
        }

        protected override Task OnActivateAsync(string key)
        {
            domainObject.Setup(DomainId.Create(key));

            return base.OnActivateAsync(key);
        }

        public async Task<J<object?>> ExecuteAsync(J<CommandRequest> request)
        {
            request.Value.ApplyContext();

            var result = await domainObject.ExecuteAsync(request.Value.Command);

            return result;
        }
    }
}