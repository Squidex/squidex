// ==========================================================================
//  ContentChangedTrigger.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    [TypeName(nameof(ContentChangedTrigger))]
    public sealed class ContentChangedTrigger : RuleTrigger
    {
        private ImmutableList<ContentChangedTriggerSchema> schemas;

        public ImmutableList<ContentChangedTriggerSchema> Schemas
        {
            get
            {
                return schemas;
            }
            set
            {
                ThrowIfFrozen();

                schemas = value;
            }
        }

        public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
