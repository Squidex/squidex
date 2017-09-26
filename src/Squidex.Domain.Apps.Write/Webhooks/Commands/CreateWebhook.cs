// ==========================================================================
//  CreateWebhook.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Webhooks.Commands
{
    public sealed class CreateWebhook : WebhookEditCommand
    {
        public string SharedSecret { get; } = RandomHash.New();

        public CreateWebhook()
        {
            WebhookId = Guid.NewGuid();
        }
    }
}
