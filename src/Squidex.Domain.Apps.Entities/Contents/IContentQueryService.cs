// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        int DefaultPageSizeGraphQl { get; }

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(QueryContext context, IReadOnlyList<Guid> ids);

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(QueryContext context, string schemaIdOrName, Q query);

        Task<IEnrichedContentEntity> FindContentAsync(QueryContext context, string schemaIdOrName, Guid id, long version = EtagVersion.Any);

        Task<ISchemaEntity> GetSchemaOrThrowAsync(QueryContext context, string schemaIdOrName);
    }
}
