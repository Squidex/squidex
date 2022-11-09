// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed class AssetFolderOperation : OperationContextBase<AssetFolderCommand, IAssetFolderEntity>
{
    public AssetFolderOperation(IServiceProvider serviceProvider, Func<IAssetFolderEntity> snapshot)
        : base(serviceProvider, snapshot)
    {
        Guard.NotNull(serviceProvider);
    }

    public static async Task<AssetFolderOperation> CreateAsync(IServiceProvider services, AssetFolderCommand command, Func<IAssetFolderEntity> snapshot)
    {
        var appProvider = services.GetRequiredService<IAppProvider>();

        var app = await appProvider.GetAppAsync(command.AppId.Id);

        if (app == null)
        {
            throw new DomainObjectNotFoundException(command.AppId.Id.ToString());
        }

        var id = command.AssetFolderId;

        return new AssetFolderOperation(services, snapshot)
        {
            App = app,
            Command = command,
            CommandId = id
        };
    }
}
