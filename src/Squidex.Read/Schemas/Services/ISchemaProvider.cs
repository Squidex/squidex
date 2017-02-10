// ==========================================================================
//  ISchemaProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Read.Schemas.Services
{
    public interface ISchemaProvider
    {
        Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid id);

        Task<ISchemaEntityWithSchema> FindSchemaByNameAsync(Guid appId, string name);

        void Remove(Guid id);
    }
}
