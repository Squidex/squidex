// ==========================================================================
//  ISchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Read.Schemas
{
    public interface ISchemaEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string Name { get; }

        bool IsPublished { get; }
        
        bool IsDeleted { get; }

        Schema Schema { get; }
    }
}
