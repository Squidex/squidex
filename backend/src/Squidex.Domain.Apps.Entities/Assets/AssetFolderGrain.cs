// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Guards;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetFolderGrain : DomainObjectGrain<AssetFolderState>, IAssetFolderGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
        private readonly IAssetQueryService assetQuery;

        public AssetFolderGrain(IStore<Guid> store, IAssetQueryService assetQuery, IActivationLimit limit, ISemanticLog log)
            : base(store, log)
        {
            Guard.NotNull(assetQuery);

            this.assetQuery = assetQuery;

            limit?.SetLimit(5000, Lifetime);
        }

        protected override Task OnActivateAsync(Guid key)
        {
            TryDelayDeactivation(Lifetime);

            return base.OnActivateAsync(key);
        }

        protected override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

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
                        await GuardAssetFolder.CanMove(c, assetQuery, Snapshot.Id, Snapshot.ParentId);

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
            RaiseEvent(SimpleMapper.Map(command, new AssetFolderCreated()));
        }

        public void Move(MoveAssetFolder command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetFolderMoved()));
        }

        public void Rename(RenameAssetFolder command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetFolderRenamed()));
        }

        public void Delete(DeleteAssetFolder command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetFolderDeleted()));
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Asset folder has already been deleted");
            }
        }
    }
}
