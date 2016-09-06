// ==========================================================================
//  IModelSchemaRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinkParrot.Read.Repositories
{
    public interface IModelSchemaRepository
    {
        Task<List<IModelSchemaEntity>> QueryAllAsync(Guid tenantId);

        Task<Guid?> FindSchemaIdAsync(Guid tenantId, string name);

        Task<EntityWithSchema> FindSchemaAsync(Guid tenantId, string name);

        Task<EntityWithSchema> FindSchemaAsync(Guid schemaId);
    }
}
