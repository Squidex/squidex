// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(FastlyAction))]
    public sealed class FastlyAction : RuleAction
    {
        public string ApiKey { get; set; }

        public string ServiceId { get; set; }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
