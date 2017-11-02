// ==========================================================================
//  WebhookAction.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(WebhookAction))]
    public sealed class WebhookAction : RuleAction
    {
        public Uri Url { get; set; }

        public string SharedSecret { get; set; }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
