// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Migrate_01.OldTriggers
{
    [TypeName(nameof(ContentChangedTrigger))]
    public sealed class ContentChangedTrigger : RuleTrigger, IMigrated<RuleTrigger>
    {
        public ReadOnlyCollection<ContentChangedTriggerSchema> Schemas { get; set; }

        public bool HandleAll { get; set; }

        public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
        {
            throw new NotSupportedException();
        }

        public override void Freeze()
        {
            base.Freeze();

            if (Schemas != null)
            {
                foreach (var schema in Schemas)
                {
                    schema.Freeze();
                }
            }
        }

        public RuleTrigger Migrate()
        {
            throw new NotImplementedException();
        }
    }
}
