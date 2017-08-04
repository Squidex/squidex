// ==========================================================================
//  WebhookResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Schemas
{
    public enum WebhookResult
    {
        Pending,
        Success,
        Failed,
        Timeout
    }
}
