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
    [TypeName(nameof(WebhookAction))]
    public sealed class WebhookAction : RuleAction
    {
        private Uri url;
        private string sharedSecret;

        public Uri Url
        {
            get
            {
                return url;
            }
            set
            {
                ThrowIfFrozen();

                url = value;
            }
        }

        public string SharedSecret
        {
            get
            {
                return sharedSecret;
            }
            set
            {
                ThrowIfFrozen();

                sharedSecret = value;
            }
        }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
