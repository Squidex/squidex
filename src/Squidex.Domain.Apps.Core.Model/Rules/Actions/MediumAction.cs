// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(MediumAction))]
    public sealed class MediumAction : RuleAction
    {
        public string AccessToken { get; set; }

        public string Tags { get; set; }

        public string Title { get; set; }

        public string CanonicalUrl { get; set; }

        public string Content { get; set; }

        public bool IsHtml { get; set; }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
