// ==========================================================================
//  IContentQueryService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents
{
    public interface IContentQueryService
    {
        Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids, string query);

        Task<(ISchemaEntity Schema, IContentEntity Content)> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id);

        Task<ISchemaEntity> FindSchemaAsync(IEntity app, string schemaIdOrName);
    }
}
