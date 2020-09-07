// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class BackupJobsDto : Resource
    {
        /// <summary>
        /// The backups.
        /// </summary>
        [LocalizedRequired]
        public BackupJobDto[] Items { get; set; }

        public static BackupJobsDto FromBackups(IEnumerable<IBackupJob> backups, Resources resources)
        {
            var result = new BackupJobsDto
            {
                Items = backups.Select(x => BackupJobDto.FromBackup(x, resources)).ToArray()
            };

            return result.CreateLinks(resources);
        }

        private BackupJobsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<BackupsController>(x => nameof(x.GetBackups), values));

            if (resources.CanCreateBackup)
            {
                AddPostLink("create", resources.Url<BackupsController>(x => nameof(x.PostBackup), values));
            }

            return this;
        }
    }
}
