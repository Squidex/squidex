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

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, IReadOnlyList<Guid> ids);

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q query);

        Task<IEnrichedContentEntity> FindContentAsync(Context context, string schemaIdOrName, Guid id, long version = EtagVersion.Any);

        Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName);
    }
}
