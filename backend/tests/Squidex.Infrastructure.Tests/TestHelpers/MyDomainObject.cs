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
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed class MyDomainObject : DomainObject<MyDomainState>
    {
        public bool Recreate { get; set; }

        public int VersionsToKeep
        {
            get => Capacity;
            set => Capacity = value;
        }

        public MyDomainObject(IPersistenceFactory<MyDomainState> factory)
           : base(factory, A.Dummy<ISemanticLog>())
        {
        }

        protected override bool CanRecreate()
        {
            return Recreate;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            if (command is CreateAuto update)
            {
                return update.Value != 99;
            }

            return true;
        }

        protected override bool CanAccept(ICommand command)
        {
            if (command is UpdateAuto update)
            {
                return update.Value != 99;
            }

            return true;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case Upsert c:
                    return Upsert(c, createAuto =>
                    {
                        RaiseEvent(new ValueChanged { Value = createAuto.Value });
                    });

                case CreateAuto c:
                    return Create(c, createAuto =>
                    {
                        RaiseEvent(new ValueChanged { Value = createAuto.Value });
                    });

                case CreateCustom c:
                    return CreateReturn(c, createCustom =>
                    {
                        RaiseEvent(new ValueChanged { Value = createCustom.Value });

                        return "CREATED";
                    });

                case UpdateAuto c:
                    return Update(c, updateAuto =>
                    {
                        RaiseEvent(new ValueChanged { Value = updateAuto.Value });
                    });

                case UpdateCustom c:
                    return UpdateReturn(c, updateCustom =>
                    {
                        RaiseEvent(new ValueChanged { Value = updateCustom.Value });

                        return "UPDATED";
                    });
                case Delete c:
                    return Update(c, delete =>
                    {
                        RaiseEvent(new Deleted());
                    });
                case DeletePermanent c:
                    return DeletePermanent(c, delete =>
                    {
                        RaiseEvent(new Deleted());
                    });
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public sealed class Delete : MyCommand
    {
    }

    public sealed class DeletePermanent : MyCommand
    {
    }

    public sealed class Upsert : MyCommand
    {
        public long Value { get; set; }
    }

    public sealed class CreateAuto : MyCommand
    {
        public long Value { get; set; }
    }

    public sealed class CreateCustom : MyCommand
    {
        public long Value { get; set; }
    }

    public sealed class UpdateAuto : MyCommand
    {
        public long Value { get; set; }
    }

    public sealed class UpdateCustom : MyCommand
    {
        public long Value { get; set; }
    }
}
