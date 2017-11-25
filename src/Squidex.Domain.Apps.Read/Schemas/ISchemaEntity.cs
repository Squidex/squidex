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
    public interface ISchemaEntity : IEntityWithAppRef, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string Name { get; }

        bool IsPublished { get; }

        bool IsDeleted { get; }

        string ScriptQuery { get; }

        string ScriptCreate { get; }

        string ScriptUpdate { get; }

        string ScriptDelete { get; }

        string ScriptChange { get; }

        Schema SchemaDef { get; }
    }
}
