// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Config.Domain
{
    public static class BackupsServices
    {
        public static void AddSquidexBackups(this IServiceCollection services)
        {
            services.AddSingletonAs<TempFolderBackupArchiveLocation>()
                .As<IBackupArchiveLocation>();

            services.AddSingletonAs<DefaultBackupArchiveStore>()
                .As<IBackupArchiveStore>();

            services.AddTransientAs<BackupService>()
                .As<IBackupService>().As<IDeleter>();

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
        }
    }
}
