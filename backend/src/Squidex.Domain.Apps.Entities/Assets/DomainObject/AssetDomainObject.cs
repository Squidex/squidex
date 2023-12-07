// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Assets;
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

public partial class AssetDomainObject : DomainObject<Asset>
{
    private readonly IServiceProvider serviceProvider;

    public AssetDomainObject(DomainId id, IPersistenceFactory<Asset> persistence, ILogger<AssetDomainObject> log,
        IServiceProvider serviceProvider)
        : base(id, persistence, log)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override bool IsDeleted(Asset snapshot)
    {
        return snapshot.IsDeleted;
    }

    protected override bool IsRecreation(IEvent @event)
    {
        return @event is AssetCreated;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is AssetCommand c && c.AppId == Snapshot.AppId && c.AssetId == Snapshot.Id;
    }

    protected override bool CanAccept(ICommand command, DomainObjectState state)
    {
        switch (state)
        {
            case DomainObjectState.Undefined:
                return command is CreateAsset;
            case DomainObjectState.Empty:
                return command is CreateAsset or UpsertAsset;
            case DomainObjectState.Deleted:
                return command is CreateAsset or UpsertAsset or DeleteAsset { Permanent: true };
            default:
                return command is not CreateAsset;
        }
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case UpsertAsset upsert:
                return ApplyReturnAsync(upsert, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    if (Version > EtagVersion.Empty && !IsDeleted(Snapshot))
                    {
                        await UpdateCore(c.AsUpdate(), operation, ct);
                    }
                    else
                    {
                        await CreateCore(c.AsCreate(), operation, ct);
                    }

                    if (c.ParentId != null && c.ParentId != Snapshot.ParentId)
                    {
                        await MoveCore(c.AsMove(c.ParentId.Value), operation, ct);
                    }

                    return Snapshot;
                }, ct);

            case CreateAsset create:
                return ApplyReturnAsync(create, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await CreateCore(c, operation, ct);

                    if (c.ParentId != Snapshot.ParentId)
                    {
                        await MoveCore(c.AsMove(), operation, ct);
                    }

                    return Snapshot;
                }, ct);

            case AnnotateAsset annotate:
                return ApplyReturnAsync(annotate, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await AnnotateCore(c, operation, ct);

                    return Snapshot;
                }, ct);

            case UpdateAsset update:
                return ApplyReturnAsync(update, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await UpdateCore(c, operation, ct);

                    return Snapshot;
                }, ct);

            case MoveAsset move:
                return ApplyReturnAsync(move, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await MoveCore(c, operation, ct);

                    return Snapshot;
                }, ct);

            case DeleteAsset { Permanent: true } delete:
                return DeletePermanentAsync(delete, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await DeleteCore(c, operation, ct);
                }, ct);

            case DeleteAsset delete:
                return ApplyAsync(delete, async (c, ct) =>
                {
                    var operation = await AssetOperation.CreateAsync(serviceProvider, c, () => Snapshot);

                    await DeleteCore(c, operation, ct);
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private async Task CreateCore(CreateAsset create, AssetOperation operation,
        CancellationToken ct)
    {
        if (!create.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(create.ParentId, ct);
        }

        if (!create.DoNotScript)
        {
            await operation.ExecuteCreateScriptAsync(create, ct);
        }

        if (create.Tags != null)
        {
            create.Tags = await operation.GetTagIdsAsync(create.Tags);
        }

        Create(create);
    }

    private async Task AnnotateCore(AnnotateAsset annotate, AssetOperation operation,
        CancellationToken ct)
    {
        if (!annotate.DoNotScript)
        {
            await operation.ExecuteAnnotateScriptAsync(annotate, ct);
        }

        if (annotate.Tags != null)
        {
            annotate.Tags = await operation.GetTagIdsAsync(annotate.Tags);
        }

        Annotate(annotate);
    }

    private async Task UpdateCore(UpdateAsset update, AssetOperation operation,
        CancellationToken ct)
    {
        if (!update.DoNotScript)
        {
            await operation.ExecuteUpdateScriptAsync(update, ct);
        }

        Update(update);
    }

    private async Task MoveCore(MoveAsset move, AssetOperation operation,
        CancellationToken ct)
    {
        if (!move.OptimizeValidation)
        {
            await operation.MustMoveToValidFolder(move.ParentId, ct);
        }

        if (!move.DoNotScript)
        {
            await operation.ExecuteMoveScriptAsync(move, ct);
        }

        Move(move);
    }

    private async Task DeleteCore(DeleteAsset delete, AssetOperation operation,
        CancellationToken ct)
    {
        if (delete.CheckReferrers)
        {
            await operation.CheckReferrersAsync(ct);
        }

        if (!delete.DoNotScript)
        {
            await operation.ExecuteDeleteScriptAsync(delete, ct);
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
