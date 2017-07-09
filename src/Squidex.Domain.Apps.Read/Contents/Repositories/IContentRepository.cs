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
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity appEntity, Guid schemaId, bool nonPublished, HashSet<Guid> ids, string odataQuery);

        Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds);

        Task<long> CountAsync(IAppEntity appEntity, Guid schemaId, bool nonPublished, HashSet<Guid> ids, string odataQuery);

        Task<IContentEntity> FindContentAsync(IAppEntity appEntity, Guid schemaId, Guid id);
    }
}
