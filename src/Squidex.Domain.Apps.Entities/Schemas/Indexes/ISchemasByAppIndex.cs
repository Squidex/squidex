﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public interface ISchemasByAppIndex : ICleanableAppGrain
    {
        Task AddSchemaAsync(Guid schemaId, string name);

        Task RemoveSchemaAsync(Guid schemaId);

        Task RebuildAsync(Dictionary<string, Guid> schemas);

        Task<Guid> GetSchemaIdAsync(string name);

        Task<List<Guid>> GetSchemaIdsAsync();
    }
}
