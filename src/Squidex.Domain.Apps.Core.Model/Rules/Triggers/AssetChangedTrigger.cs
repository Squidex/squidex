// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    [TypeName(nameof(AssetChangedTrigger))]
    public sealed class AssetChangedTrigger : RuleTrigger
    {
        public bool SendCreate { get; set; }

        public bool SendUpdate { get; set; }

        public bool SendRename { get; set; }

        public bool SendDelete { get; set; }

        public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
