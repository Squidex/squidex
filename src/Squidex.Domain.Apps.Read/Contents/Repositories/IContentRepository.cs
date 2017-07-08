// ==========================================================================
//  IContentRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Read.Apps;

namespace Squidex.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<IReadOnlyList<IContentEntity>> QueryAsync(Guid schemaId, bool nonPublished, HashSet<Guid> ids, string odataQuery, IAppEntity appEntity);

        Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds);

        Task<long> CountAsync(Guid schemaId, bool nonPublished, HashSet<Guid> ids, string odataQuery, IAppEntity appEntity);

        Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id, IAppEntity appEntity);
    }
}
