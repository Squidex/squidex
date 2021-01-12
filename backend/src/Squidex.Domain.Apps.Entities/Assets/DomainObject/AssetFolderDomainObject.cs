// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed partial class AssetFolderDomainObject : DomainObject<AssetFolderDomainObject.State>
    {
        private readonly IAssetQueryService assetQuery;

        public AssetFolderDomainObject(IStore<DomainId> store, ISemanticLog log,
            IAssetQueryService assetQuery)
            : base(store, log)
        {
            Guard.NotNull(assetQuery, nameof(assetQuery));

            this.assetQuery = assetQuery;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is AssetFolderCommand;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is AssetFolderCommand assetFolderCommand &&
                Equals(assetFolderCommand.AppId, Snapshot.AppId) &&
                Equals(assetFolderCommand.AssetFolderId, Snapshot.Id);
        }

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAssetFolder createAssetFolder:
                    return CreateReturnAsync(createAssetFolder, async c =>
                    {
                        await GuardAssetFolder.CanCreate(c, assetQuery);

                        Create(c);

                        return Snapshot;
                    });

                case MoveAssetFolder moveAssetFolder:
                    return UpdateReturnAsync(moveAssetFolder, async c =>
                    {
                        await GuardAssetFolder.CanMove(c, Snapshot, assetQuery);

                        Move(c);

                        return Snapshot;
                    });

                case RenameAssetFolder renameAssetFolder:
                    return UpdateReturn(renameAssetFolder, c =>
                    {
                        GuardAssetFolder.CanRename(c);

                        Rename(c);

                        return Snapshot;
                    });

                case DeleteAssetFolder deleteAssetFolder:
                    return Update(deleteAssetFolder, c =>
                    {
                        GuardAssetFolder.CanDelete(c);

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        public void Create(CreateAssetFolder command)
        {
            Raise(command, new AssetFolderCreated());
        }

        public void Move(MoveAssetFolder command)
        {
            Raise(command, new AssetFolderMoved());
        }

        public void Rename(RenameAssetFolder command)
        {
            Raise(command, new AssetFolderRenamed());
        }

        public void Delete(DeleteAssetFolder command)
        {
            Raise(command, new AssetFolderDeleted());
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            SimpleMapper.Map(command, @event);

            @event.AppId ??= Snapshot.AppId;

            RaiseEvent(Envelope.Create(@event));
        }
    }
}
