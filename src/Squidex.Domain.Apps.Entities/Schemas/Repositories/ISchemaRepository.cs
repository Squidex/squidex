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

namespace Squidex.Domain.Apps.Entities.Schemas.Repositories
{
    public interface ISchemaRepository
    {
        Task<string> FindSchemaNameAsync(Guid schemaId);

        Task<IReadOnlyList<string>> QuerySchemaNamesAsync(Guid appId);
    }
}
