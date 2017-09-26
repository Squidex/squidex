// ==========================================================================
//  IWebhookEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public interface IWebhookEventEntity : IEntity
    {
        WebhookJob Job { get; }

        Instant? NextAttempt { get; }

        WebhookResult Result { get; }

        WebhookJobResult JobResult { get; }

        int NumCalls { get; }

        string LastDump { get; }
    }
}
