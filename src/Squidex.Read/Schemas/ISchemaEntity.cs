// ==========================================================================
//  ISchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Schemas
{
    public interface ISchemaEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string Name { get; }

        string Label { get; }

        bool IsPublished { get; }
        
        bool IsDeleted { get; }
    }
}
