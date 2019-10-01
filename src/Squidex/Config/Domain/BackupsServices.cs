// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
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

            services.AddTransientAs<BackupApps>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupAssets>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupContents>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupRules>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupSchemas>()
                .As<BackupHandler>();
        }
    }
}