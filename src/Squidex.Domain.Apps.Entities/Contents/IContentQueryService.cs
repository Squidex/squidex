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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, string query);

        Task<IContentEntity> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id, long version = EtagVersion.Any);

        Task ThrowIfSchemaNotExistsAsync(IAppEntity app, string schemaIdOrName);
    }
}
