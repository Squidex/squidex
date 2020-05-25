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
        Task<TextContentState?> GetAsync(DomainId appId, DomainId contentId);

        Task SetAsync(DomainId appId, TextContentState state);

        Task RemoveAsync(DomainId appId, DomainId contentId);

        Task ClearAsync();
    }
}
