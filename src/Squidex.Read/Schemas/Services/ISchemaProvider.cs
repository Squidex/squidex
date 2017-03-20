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
        Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid id, bool provideDeleted = false);

        Task<ISchemaEntityWithSchema> FindSchemaByNameAsync(Guid appId, string name);
    }
}
