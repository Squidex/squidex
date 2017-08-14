// ==========================================================================
//  WebhookJobResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public enum WebhookJobResult
    {
        Pending,
        Success,
        Retry,
        Failed
    }
}
