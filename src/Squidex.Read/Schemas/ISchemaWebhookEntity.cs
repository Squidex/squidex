// ==========================================================================
//  ISchemaWebhookEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read.Schemas
{
    public interface ISchemaWebhookEntity
    {
        Guid Id { get; }

        Guid SchemaId { get; }

        Uri Url { get; }

        string SecurityToken { get; }
    }
}
