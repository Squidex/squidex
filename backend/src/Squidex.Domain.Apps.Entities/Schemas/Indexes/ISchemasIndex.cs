// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public interface ISchemasIndex
    {
        Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool allowDeleted = false);

        Task<ISchemaEntity?> GetSchemaByNameAsync(DomainId appId, string name, bool allowDeleted = false);

        Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId, bool allowDeleted = false);

        Task RebuildAsync(DomainId appId, Dictionary<string, DomainId> schemas);
    }
}