// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Text.State;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexerState
    {
        Task<ContentState?> GetAsync(Guid contentId);

        Task SetAsync(ContentState state);

        Task RemoveAsync(Guid contentId);
    }
}
