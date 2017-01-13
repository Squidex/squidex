// ==========================================================================
//  ISchemaProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Read.Schemas.Repositories;

namespace Squidex.Read.Schemas.Services
{
    public interface ISchemaProvider
    {
        Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid schemaId);

        Task<ISchemaEntityWithSchema> FindSchemaByNameAsync(Guid appId, string name);
    }
}
