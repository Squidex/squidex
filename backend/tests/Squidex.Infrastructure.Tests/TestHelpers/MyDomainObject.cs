// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.TestHelpers;

public sealed class MyDomainObject : DomainObject<MyDomainState>
{
    public bool Recreate { get; set; }

    public bool RecreateEvent { get; set; }

    public MyDomainObject(DomainId id, IPersistenceFactory<MyDomainState> factory)
       : base(id, factory, A.Dummy<ILogger>())
    {
    }

    protected override bool CanRecreate(IEvent @event)
    {
        return RecreateEvent && @event is ValueChanged;
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

    protected override bool IsDeleted(MyDomainState snapshot)
    {
        return snapshot.IsDeleted;
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case Upsert c:
                return Upsert(c, createAuto =>
                {
                    RaiseEvent(new ValueChanged { Value = createAuto.Value });
                }, ct);

            case CreateAuto c:
                return Create(c, createAuto =>
                {
                    RaiseEvent(new ValueChanged { Value = createAuto.Value });
                }, ct);

            case CreateCustom c:
                return CreateReturn(c, createCustom =>
                {
                    RaiseEvent(new ValueChanged { Value = createCustom.Value });

                    return "CREATED";
                }, ct);

            case UpdateAuto c:
                return Update(c, updateAuto =>
                {
                    RaiseEvent(new ValueChanged { Value = updateAuto.Value });
                }, ct);

            case UpdateCustom c:
                return UpdateReturn(c, updateCustom =>
                {
                    RaiseEvent(new ValueChanged { Value = updateCustom.Value });

                    return "UPDATED";
                }, ct);

            case Delete c:
                return Update(c, delete =>
                {
                    RaiseEvent(new Deleted());
                }, ct);

            case DeletePermanent c:
                return DeletePermanent(c, delete =>
                {
                    RaiseEvent(new Deleted());
                }, ct);

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
