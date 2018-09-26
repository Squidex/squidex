// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.TestHelpers
{
    public class MyGrain : DomainObjectGrain<MyDomainState>
    {
        public MyGrain(IStore<Guid> store)
            : base(store, A.Dummy<ISemanticLog>())
        {
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            return Task.FromResult<object>(null);
        }

        protected override MyDomainState OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot;
        }
    }
}
