// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexerGrain : IGrainWithGuidKey
    {
        Task DeleteAsync(Guid id);

        Task IndexAsync(Guid id, J<IndexData> data, bool onlyDraft);

        Task CopyAsync(Guid id, bool fromDraft);

        Task<List<Guid>> SearchAsync(string queryText, SearchContext context);
    }
}