// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Migrations.Migrations.Backup;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Domain;

public static class BackupsServices
{
    public static void AddSquidexBackups(this IServiceCollection services)
    {
        services.AddHttpClient("Backup", options =>
        {
            options.Timeout = TimeSpan.FromHours(1);
        });

        services.AddSingletonAs<TempFolderBackupArchiveLocation>()
            .As<IBackupArchiveLocation>();

        services.AddSingletonAs<DefaultBackupHandlerFactory>()
            .As<IBackupHandlerFactory>();

        services.AddSingletonAs<DefaultBackupArchiveStore>()
            .As<IBackupArchiveStore>();

        services.AddTransientAs<BackupApps>()
            .As<IBackupHandler>();

        services.AddTransientAs<BackupAssets>()
            .As<IBackupHandler>();

        services.AddTransientAs<BackupContents>()
            .As<IBackupHandler>();

        services.AddTransientAs<BackupRules>()
            .As<IBackupHandler>();

        services.AddTransientAs<BackupSchemas>()
            .As<IBackupHandler>();

        services.AddTransientAs<RestoreJob>()
            .AsSelf();

        services.AddTransientAs<ConvertBackup>()
            .As<IMigration>();
    }
}
