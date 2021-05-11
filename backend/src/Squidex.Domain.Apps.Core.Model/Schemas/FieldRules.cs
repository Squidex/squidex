// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class FieldRules : ImmutableList<FieldRule>
    {
        public static readonly FieldRules Empty = new FieldRules(new List<FieldRule>());

        public FieldRules()
        {
        }

        public FieldRules(IList<FieldRule> list)
            : base(list)
        {
        }

        public static FieldRules Create(params FieldRule[] rules)
        {
            return new FieldRules(rules.ToArray());
        }
    }
}
