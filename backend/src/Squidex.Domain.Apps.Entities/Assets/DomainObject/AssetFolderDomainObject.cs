// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed partial class AssetFolderDomainObject : DomainObject<AssetFolderDomainObject.State>
    {
        private readonly IServiceProvider serviceProvider;

        public AssetFolderDomainObject(IPersistenceFactory<State> factory, ISemanticLog log,
            IServiceProvider serviceProvider)
            : base(factory, log)
        {
            this.serviceProvider = serviceProvider;
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

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAssetFolder c:
                    return CreateReturnAsync(c, async create =>
                    {
                        await CreateCore(create, c);

                        return Snapshot;
                    });

                case MoveAssetFolder move:
                    return UpdateReturnAsync(move, async c =>
                    {
                        await MoveCore(c);

                        return Snapshot;
                    });

                case RenameAssetFolder rename:
                    return UpdateReturnAsync(rename, async c =>
                    {
                        await RenameCore(c);

                        return Snapshot;
                    });

                case DeleteAssetFolder delete:
                    return Update(delete, c =>
                    {
                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
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
}
