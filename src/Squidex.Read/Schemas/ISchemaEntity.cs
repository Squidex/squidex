// ==========================================================================
//  ISchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using System.Collections.Generic;

namespace Squidex.Read.Schemas
{
    public interface ISchemaEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string Name { get; }

        bool IsPublished { get; }
        
        bool IsDeleted { get; }

        Schema Schema { get; }

        IEnumerable<ISchemaWebhookEntity> Webhooks { get; }
    }
}
