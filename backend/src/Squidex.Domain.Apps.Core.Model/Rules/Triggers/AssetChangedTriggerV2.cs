// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    [TypeName(nameof(AssetChangedTriggerV2))]
    public sealed class AssetChangedTriggerV2 : RuleTrigger
    {
        public string Condition { get; set; }

        public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
