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
using Microsoft.OData.UriParser;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, bool nonPublished, HashSet<Guid> ids, ODataUriParser odataQuery);

        Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds);

        Task<long> CountAsync(IAppEntity app, ISchemaEntity schema, bool nonPublished, HashSet<Guid> ids, ODataUriParser odataQuery);

        Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id);
    }
}
