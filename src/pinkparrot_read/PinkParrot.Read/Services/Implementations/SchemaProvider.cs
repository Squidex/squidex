// ==========================================================================
//  SchemaProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Read.Services.Implementations
{
    public class SchemaProvider : ISchemaProvider
    {
        public Task<Guid> FindSchemaIdByNameAsync(string name)
        {
            return Task.FromResult(Guid.Empty);
        }
    }
}
