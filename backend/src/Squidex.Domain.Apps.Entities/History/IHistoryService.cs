// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.History
{
    public interface IHistoryService
    {
        Task<IReadOnlyList<ParsedHistoryEvent>> QueryByChannelAsync(Guid appId, string channelPrefix, int count);
    }
}
