﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History.Repositories
{
    public interface IHistoryEventRepository
    {
        Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId appId, string channelPrefix, int count);

        Task InsertAsync(HistoryEvent item);

        Task ClearAsync();
    }
}
