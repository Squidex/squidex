// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class BackupHistory : BackupHandler
    {
        private readonly IHistoryEventRepository historyEventRepository;

        public BackupHistory(IHistoryEventRepository historyEventRepository)
        {
            Guard.NotNull(historyEventRepository, nameof(historyEventRepository));

            this.historyEventRepository = historyEventRepository;
        }

        public override Task RemoveAsync(Guid appId)
        {
            return historyEventRepository.RemoveAsync(appId);
        }
    }
}
