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
        Task<Guid> FindSchemaIdAsync(Guid appId, string name);

        Task<IReadOnlyList<Guid>> QuerySchemaIdsAsync(Guid appId);
    }
}
