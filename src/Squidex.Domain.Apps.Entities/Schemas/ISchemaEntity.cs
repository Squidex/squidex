﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public interface ISchemaEntity :
        IEntity,
        IEntityWithAppRef,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
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
