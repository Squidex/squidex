// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<(ISchemaEntity Schema, IResultList<IContentEntity> Contents)> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids);

        Task<(ISchemaEntity Schema, IResultList<IContentEntity> Contents)> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, string query);

        Task<(ISchemaEntity Schema, IContentEntity Content)> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id, long version = EtagVersion.Any);

        Task<ISchemaEntity> FindSchemaAsync(IAppEntity app, string schemaIdOrName);
    }
}
