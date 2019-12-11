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

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface ITextIndexerGrain : IGrainWithGuidKey
    {
        Task<bool> DeleteAsync(Guid id);

        Task<bool> CopyAsync(Guid id, bool fromDraft);

        Task<bool> IndexAsync(Update update);

        Task<List<Guid>> SearchAsync(string queryText, SearchContext context);
    }
}