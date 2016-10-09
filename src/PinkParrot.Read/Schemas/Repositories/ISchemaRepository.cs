// ==========================================================================
//  ISchemaRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinkParrot.Read.Schemas.Repositories
{
    public interface ISchemaRepository
    {
        Task<List<ISchemaEntity>> QueryAllAsync(Guid tenantId);

        Task<Guid?> FindSchemaIdAsync(Guid tenantId, string name);

        Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid tenantId, string name);

        Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid schemaId);
    }
}
