// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed partial class AssetFolderDomainObject : DomainObject<AssetFolderDomainObject.State>
{
    private readonly IServiceProvider serviceProvider;

    public AssetFolderDomainObject(DomainId id, IPersistenceFactory<State> persistence, ILogger<AssetFolderDomainObject> log,
        IServiceProvider serviceProvider)
        : base(id, persistence, log)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override bool IsDeleted(State snapshot)
    {
        return Snapshot.IsDeleted;
    }

    protected override bool CanAcceptCreation(ICommand command)
    {
        return command is AssetFolderCommandBase;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is AssetFolderCommand assetFolderCommand &&
            Equals(assetFolderCommand.AppId, Snapshot.AppId) &&
            Equals(assetFolderCommand.AssetFolderId, Snapshot.Id);
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case CreateAssetFolder create:
                return CreateReturnAsync(create, async (c, ct) =>
                {
                    await CreateCore(c, c);

                    return Snapshot;
                }, ct);

            case MoveAssetFolder move:
                return UpdateReturnAsync(move, async (c, ct) =>
                {
                    await MoveCore(c);

                    return Snapshot;
                }, ct);

            case RenameAssetFolder rename:
                return UpdateReturnAsync(rename, async (c, ct) =>
                {
                    await RenameCore(c);

                    return Snapshot;
                }, ct);

            case DeleteAssetFolder delete:
                return Update(delete, c =>
                {
                    Delete(c);
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private async Task CreateCore(CreateAssetFolder create, CreateAssetFolder c)
    {
        var operation = await AssetFolderOperation.CreateAsync(serviceProvider, c, () => Snapshot);

        operation.MustHaveName(c.FolderName);

        if (!c.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(c.ParentId);
        }

        Create(create);
    }

    private async Task MoveCore(MoveAssetFolder c)
    {
        var operation = await AssetFolderOperation.CreateAsync(serviceProvider, c, () => Snapshot);

        if (!c.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(c.ParentId);
        }

        Move(c);
    }

    private async Task RenameCore(RenameAssetFolder c)
    {
        var operation = await AssetFolderOperation.CreateAsync(serviceProvider, c, () => Snapshot);

        operation.MustHaveName(c.FolderName);

        Rename(c);
    }

    private void Create(CreateAssetFolder command)
    {
        Raise(command, new AssetFolderCreated());
    }

    private void Move(MoveAssetFolder command)
    {
        Raise(command, new AssetFolderMoved());
    }

    private void Rename(RenameAssetFolder command)
    {
        Raise(command, new AssetFolderRenamed());
    }

    private void Delete(DeleteAssetFolder command)
    {
        Raise(command, new AssetFolderDeleted());
    }

    private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
    {
        RaiseEvent(Envelope.Create(SimpleMapper.Map(command, @event)));
    }
}
