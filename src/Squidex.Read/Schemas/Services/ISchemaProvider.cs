// ==========================================================================
//  ISchemaProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Read.Schemas.Services
{
    public interface ISchemaProvider
    {
        Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid id, bool provideDeleted = false);

        Task<ISchemaEntityWithSchema> FindSchemaByNameAsync(Guid appId, string name);
        
        void Remove(NamedId<Guid> appId, NamedId<Guid> schemaId);
    }
}
