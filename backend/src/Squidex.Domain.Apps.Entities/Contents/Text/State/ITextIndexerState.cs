// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public interface ITextIndexerState
    {
        Task<Dictionary<DomainId, TextContentState>> GetAsync(HashSet<DomainId> ids);

        Task SetAsync(List<TextContentState> updates);

        Task ClearAsync();
    }
}
