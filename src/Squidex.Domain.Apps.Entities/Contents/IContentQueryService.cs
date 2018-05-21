// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, HashSet<Guid> ids);

        Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, string query);

        Task<IContentEntity> FindContentAsync(QueryContext context, Guid id, long version = EtagVersion.Any);

        Task ThrowIfSchemaNotExistsAsync(QueryContext context);
    }
}
