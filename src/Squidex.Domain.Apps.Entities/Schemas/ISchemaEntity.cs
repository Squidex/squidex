// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public interface ISchemaEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        NamedId<Guid> AppId { get; }

        string Name { get; }

        string Category { get; }

        bool IsSingleton { get; }

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
