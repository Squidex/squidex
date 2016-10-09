// ==========================================================================
//  ISchemaProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Read.Schemas.Services
{
    public interface ISchemaProvider
    {
        Task<Guid?> FindSchemaIdByNameAsync(Guid tenantId, string name);
    }
}
