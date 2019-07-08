// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class BackupJobDto : Resource
    {
        /// <summary>
        /// The id of the backup job.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The time when the job has been started.
        /// </summary>
        public Instant Started { get; set; }

        /// <summary>
        /// The time when the job has been stopped.
        /// </summary>
        public Instant? Stopped { get; set; }

        /// <summary>
        /// The number of handled events.
        /// </summary>
        public int HandledEvents { get; set; }

        /// <summary>
        /// The number of handled assets.
        /// </summary>
        public int HandledAssets { get; set; }

        /// <summary>
        /// The status of the operation.
        /// </summary>
        public JobStatus Status { get; set; }

        public static BackupJobDto FromBackup(IBackupJob backup, ApiController controller, string app)
        {
            var result = SimpleMapper.Map(backup, new BackupJobDto());

            return result.CreateLinks(controller, app);
        }

        private BackupJobDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app, id = Id };

            if (controller.HasPermission(Permissions.AppBackupsDelete, app))
            {
                AddDeleteLink("delete", controller.Url<BackupsController>(x => nameof(x.DeleteBackup), values));
            }

            AddGetLink("download", controller.Url<BackupContentController>(x => nameof(x.GetBackupContent), values));

            return this;
        }
    }
}
