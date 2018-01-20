// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.History.Repositories
{
    public interface IHistoryEventRepository
    {
        Task<IReadOnlyList<IHistoryEventEntity>> QueryByChannelAsync(Guid appId, string channelPrefix, int count);
    }
}
