// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class BackupJobsDto : Resource
    {
        /// <summary>
        /// The backups.
        /// </summary>
        [Required]
        public BackupJobDto[] Items { get; set; }

        public static BackupJobsDto FromBackups(IEnumerable<IBackupJob> backups, ApiController controller, string app)
        {
            var result = new BackupJobsDto
            {
                Items = backups.Select(x => BackupJobDto.FromBackup(x, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        private BackupJobsDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<BackupsController>(x => nameof(x.GetBackups), values));

            if (controller.HasPermission(Permissions.AppBackupsCreate, app))
            {
                AddPostLink("create", controller.Url<BackupsController>(x => nameof(x.PostBackup), values));
            }

            return this;
        }
    }
}
