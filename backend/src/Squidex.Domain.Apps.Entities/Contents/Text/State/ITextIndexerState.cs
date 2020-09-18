// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public interface ITextIndexerState
    {
        Task<Dictionary<Guid, TextContentState>> GetAsync(HashSet<Guid> ids);

        Task SetAsync(List<TextContentState> updates);

        Task ClearAsync();
    }
}
