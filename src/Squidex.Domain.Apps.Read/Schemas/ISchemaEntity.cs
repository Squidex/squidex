// ==========================================================================
//  ISchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Read.Schemas
{
    public interface ISchemaEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string Name { get; }

        bool IsPublished { get; }

        bool IsDeleted { get; }

        string ScriptQuery { get; }

        string ScriptCreate { get; }

        string ScriptUpdate { get; }

        string ScriptDelete { get; }

        string ScriptPublish { get; }

        string ScriptUnpublish { get; }

        Schema SchemaDef { get; }
    }
}
