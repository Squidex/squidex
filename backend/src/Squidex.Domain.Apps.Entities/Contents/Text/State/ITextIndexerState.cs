// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public interface ITextIndexerState
    {
        Task<TextContentState?> GetAsync(Guid contentId);

        Task SetAsync(TextContentState state);

        Task RemoveAsync(Guid contentId);

        Task ClearAsync();
    }
}
