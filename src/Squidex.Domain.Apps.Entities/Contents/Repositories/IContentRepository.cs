// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery);

        Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> ids);

        Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id);

        Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id, long version);

        Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback);
    }
}
