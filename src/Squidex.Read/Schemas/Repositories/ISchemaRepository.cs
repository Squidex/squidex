// ==========================================================================
//  ISchemaRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Read.Schemas.Repositories
{
    public interface ISchemaRepository
    {
        Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId);

        Task<ISchemaEntity> FindSchemaAsync(Guid appId, string name);

        Task<ISchemaEntity> FindSchemaAsync(Guid schemaId);
    }
}
