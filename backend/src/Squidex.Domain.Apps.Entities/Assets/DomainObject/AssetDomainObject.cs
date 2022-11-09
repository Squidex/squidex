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

public partial class AssetDomainObject : DomainObject<AssetDomainObject.State>
{
    private readonly IServiceProvider serviceProvider;

    public AssetDomainObject(DomainId id, IPersistenceFactory<State> persistence, ILogger<AssetDomainObject> log,
        IServiceProvider serviceProvider)
        : base(id, persistence, log)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override bool IsDeleted(State snapshot)
    {
        return snapshot.IsDeleted;
    }

    protected override bool CanRecreate()
    {
        return true;
    }

    protected override bool CanRecreate(IEvent @event)
    {
        return @event is AssetCreated;
    }

    protected override bool CanAcceptCreation(ICommand command)
    {
        return command is AssetCommandBase;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is AssetCommand assetCommand &&
            Equals(assetCommand.AppId, Snapshot.AppId) &&
            Equals(assetCommand.AssetId, Snapshot.Id);
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case UpsertAsset upsert:
                return UpsertReturnAsync(upsert, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    if (Version > EtagVersion.Empty && !IsDeleted(Snapshot))
                    {
                        await UpdateCore(c.AsUpdate(), operation);
                    }
                    else
                    {
                        await CreateCore(c.AsCreate(), operation);
                    }

                    if (Is.OptionalChange(Snapshot.ParentId, c.ParentId))
                    {
                        await MoveCore(c.AsMove(c.ParentId.Value), operation);
                    }

                    return Snapshot;
                }, ct);

            case CreateAsset create:
                return CreateReturnAsync(create, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await CreateCore(c, operation);

                    if (Is.Change(Snapshot.ParentId, c.ParentId))
                    {
                        await MoveCore(c.AsMove(), operation);
                    }

                    return Snapshot;
                }, ct);

            case AnnotateAsset annotate:
                return UpdateReturnAsync(annotate, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await AnnotateCore(c, operation);

                    return Snapshot;
                }, ct);

            case UpdateAsset update:
                return UpdateReturnAsync(update, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await UpdateCore(c, operation);

                    return Snapshot;
                }, ct);

            case MoveAsset move:
                return UpdateReturnAsync(move, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await MoveCore(c, operation);

                    return Snapshot;
                }, ct);

            case DeleteAsset { Permanent: true } delete:
                return DeletePermanentAsync(delete, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await DeleteCore(c, operation);
                }, ct);

            case DeleteAsset delete:
                return UpdateAsync(delete, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await DeleteCore(c, operation);
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private async Task CreateCore(CreateAsset create, AssetOperation operation)
    {
        if (!create.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(create.ParentId);
        }

        if (!create.DoNotScript)
        {
            await operation.ExecuteCreateScriptAsync(create);
        }

        if (create.Tags != null)
        {
            create.Tags = await operation.GetTagIdsAsync(create.Tags);
        }

        Create(create);
    }

    private async Task AnnotateCore(AnnotateAsset annotate, AssetOperation operation)
    {
        if (!annotate.DoNotScript)
        {
            await operation.ExecuteAnnotateScriptAsync(annotate);
        }

        if (annotate.Tags != null)
        {
            annotate.Tags = await operation.GetTagIdsAsync(annotate.Tags);
        }

        Annotate(annotate);
    }

    private async Task UpdateCore(UpdateAsset update, AssetOperation operation)
    {
        if (!update.DoNotScript)
        {
            await operation.ExecuteUpdateScriptAsync(update);
        }

        Update(update);
    }

    private async Task MoveCore(MoveAsset move, AssetOperation operation)
    {
        if (!move.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(move.ParentId);
        }

        if (!move.DoNotScript)
        {
            await operation.ExecuteMoveScriptAsync(move);
        }

        Move(move);
    }

    private async Task DeleteCore(DeleteAsset delete, AssetOperation operation)
    {
        if (delete.CheckReferrers)
        {
            await operation.CheckReferrersAsync();
        }

        if (!delete.DoNotScript)
        {
            await operation.ExecuteDeleteScriptAsync(delete);
        }

        Delete(delete);
    }

    private void Create(CreateAsset command)
    {
        Raise(command, new AssetCreated
        {
            MimeType = command.File.MimeType,
            FileName = command.File.FileName,
            FileSize = command.File.FileSize,
            Slug = command.File.FileName.ToAssetSlug()
        });
    }

    private void Update(UpdateAsset command)
    {
        Raise(command, new AssetUpdated
        {
            MimeType = command.File.MimeType,
            FileVersion = Snapshot.FileVersion + 1,
            FileSize = command.File.FileSize
        });
    }

    private void Annotate(AnnotateAsset command)
    {
        Raise(command, new AssetAnnotated());
    }

    private void Move(MoveAsset command)
    {
        Raise(command, new AssetMoved());
    }

    private void Delete(DeleteAsset command)
    {
        Raise(command, new AssetDeleted { OldTags = Snapshot.Tags, DeletedSize = Snapshot.TotalSize });
    }

    private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
    {
        RaiseEvent(Envelope.Create(SimpleMapper.Map(command, @event)));
    }
}
