// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(SlackAction))]
    public sealed class SlackAction : RuleAction
    {
        private Uri webhookUrl;
        private string text;

        public Uri WebhookUrl
        {
            get
            {
                return webhookUrl;
            }
            set
            {
                ThrowIfFrozen();

                webhookUrl = value;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                ThrowIfFrozen();

                text = value;
            }
        }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
