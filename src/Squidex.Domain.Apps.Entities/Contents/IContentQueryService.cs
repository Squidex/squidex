// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<IResultList<IContentEntity>> QueryAsync(ContentQueryContext context, Q query);

        Task<IContentEntity> FindContentAsync(ContentQueryContext context, Guid id, long version = EtagVersion.Any);

        Task ThrowIfSchemaNotExistsAsync(ContentQueryContext context);
    }
}
