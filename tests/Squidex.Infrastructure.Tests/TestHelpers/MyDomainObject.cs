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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed class MyDomainObject : DomainObjectGrain<MyDomainState>
    {
        public MyDomainObject(IStore<Guid> store)
           : base(store, A.Dummy<ISemanticLog>())
        {
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAuto createAuto:
                    return Create(createAuto, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });
                    });

                case CreateCustom createCustom:
                    return CreateReturn(createCustom, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });

                        return "CREATED";
                    });

                case UpdateAuto updateAuto:
                    return Update(updateAuto, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });
                    });

                case UpdateCustom updateCustom:
                    return UpdateReturn(updateCustom, c =>
                    {
                        RaiseEvent(new ValueChanged { Value = c.Value });

                        return "UPDATED";
                    });
            }

            return Task.FromResult<object>(null);
        }
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
}
