// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public interface ISchemasIndex
    {
        Task<ISchemaEntity?> GetSchemaAsync(Guid appId, Guid id, bool canCache);

        Task<ISchemaEntity?> GetSchemaByNameAsync(Guid appId, string name, bool canCache);

        Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId);

        Task RebuildAsync(Guid appId, Dictionary<string, Guid> schemas);
    }
}