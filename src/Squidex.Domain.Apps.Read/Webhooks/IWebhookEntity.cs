// ==========================================================================
//  IWebhookEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Webhooks;

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public interface IWebhookEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        Uri Url { get; }

        long TotalSucceeded { get; }

        long TotalFailed { get; }

        long TotalTimedout { get; }

        long TotalRequestTime { get; }

        string SharedSecret { get; }

        IEnumerable<WebhookSchema> Schemas { get; }
    }
}
