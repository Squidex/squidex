// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public interface ITextIndexerState
    {
        Task<TextContentState?> GetAsync(DomainId contentId);

        Task SetAsync(TextContentState state);

        Task RemoveAsync(DomainId contentId);

        Task ClearAsync();
    }
}
