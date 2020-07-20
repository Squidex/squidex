// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public interface ILuceneTextIndexGrain : IGrainWithStringKey
    {
        Task IndexAsync(NamedId<DomainId> schemaId, Immutable<IndexCommand[]> updates);

        Task<List<DomainId>> SearchAsync(string queryText, SearchFilter? filter, SearchContext context);
    }
}