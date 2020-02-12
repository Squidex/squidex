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
using Orleans.Concurrency;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public interface ILuceneTextIndexGrain : IGrainWithGuidKey
    {
        Task IndexAsync(NamedId<Guid> schemaId, Immutable<IndexCommand[]> updates);

        Task<List<Guid>> SearchAsync(string queryText, Guid schemaId, SearchContext context);
    }
}