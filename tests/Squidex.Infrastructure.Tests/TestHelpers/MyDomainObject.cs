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
    public sealed class MyDomainObject : DomainObjectGrain<MyDomainState>
    {
        public sealed class ValueChanged : IEvent
        {
            public int Value { get; set; }
        }

        public sealed class CreateAuto : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class CreateCustom : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class UpdateAuto : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class UpdateCustom : MyCommand
        {
            public int Value { get; set; }
        }

        public MyDomainObject(IStore<Guid> store)
           : base(store, A.Dummy<ISemanticLog>())
        {
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAuto createAuto:
                    return CreateAsync(createAuto, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });
                    });

                case CreateCustom createCustom:
                    return CreateReturnAsync(createCustom, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });

                        return "CREATED";
                    });

                case UpdateAuto updateAuto:
                    return UpdateAsync(updateAuto, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });
                    });

                case UpdateCustom updateCustom:
                    return UpdateAsync(updateCustom, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });

                        return "UPDATED";
                    });
            }

            return Task.FromResult<object>(null);
        }

        protected override MyDomainState OnEvent(Envelope<IEvent> @event)
        {
            return new MyDomainState { Value = ((ValueChanged)@event.Payload).Value };
        }
    }
}
