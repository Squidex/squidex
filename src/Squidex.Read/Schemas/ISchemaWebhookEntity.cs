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
        Guid SchemaId { get; }

        Guid Id { get; }

        Uri Url { get; }

        string SharedSecret { get; }
    }
}
