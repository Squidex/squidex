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
        Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false);

        Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name, bool allowDeleted = false);

        Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId, bool allowDeleted = false);

        Task RebuildAsync(Guid appId, Dictionary<string, Guid> schemas);
    }
}