// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(ElasticSearchAction))]
    public sealed class ElasticSearchAction : RuleAction
    {
        public Uri Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string IndexName { get; set; }

        public string IndexType { get; set; }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
