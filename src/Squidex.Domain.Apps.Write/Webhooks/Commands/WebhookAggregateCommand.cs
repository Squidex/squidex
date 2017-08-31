// ==========================================================================
//  WebhookAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

// ReSharper disable MemberCanBeProtected.Global

namespace Squidex.Domain.Apps.Write.Webhooks.Commands
{
    public abstract class WebhookAggregateCommand : AppCommand, IAggregateCommand
    {
        public Guid WebhookId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return WebhookId; }
        }
    }
}
