// ==========================================================================
//  ISchemaWebhookUrlEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Read.Schemas
{
    public interface ISchemaWebhookUrlEntity
    {
        Guid Id { get; }

        Uri Url { get; }

        string SharedSecret { get; }
    }
}