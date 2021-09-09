// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetOperation : OperationContextBase<AssetCommand, IAssetEntity>
    {
        public AssetOperation(IServiceProvider serviceProvider, Func<IAssetEntity> snapshot)
            : base(serviceProvider, snapshot)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
        }

        public static async Task<AssetOperation> CreateAsync(IServiceProvider services, AssetCommand command, Func<IAssetEntity> snapshot)
        {
            var appProvider = services.GetRequiredService<IAppProvider>();

            var app = await appProvider.GetAppAsync(command.AppId.Id);

            if (app == null)
            {
                throw new DomainObjectNotFoundException(command.AppId.Id.ToString());
            }

            var id = command.AssetId;

            return new AssetOperation(services, snapshot)
            {
                App = app,
                Command = command,
                CommandId = id
            };
        }
    }
}
