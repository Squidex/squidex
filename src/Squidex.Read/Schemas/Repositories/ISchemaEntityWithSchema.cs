// ==========================================================================
//  EntityWithSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;

namespace PinkParrot.Read.Schemas.Repositories
{
    public interface ISchemaEntityWithSchema : ISchemaEntity
    {
        Schema Schema { get; }
    }
}
