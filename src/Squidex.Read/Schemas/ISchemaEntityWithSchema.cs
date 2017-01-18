// ==========================================================================
//  ISchemaEntityWithSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Read.Schemas
{
    public interface ISchemaEntityWithSchema : ISchemaEntity
    {
        Schema Schema { get; }
    }
}
