// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class FieldRules : ReadOnlyCollection<FieldRule>
    {
        private static readonly List<FieldRule> EmptyRules = new List<FieldRule>();

        public static readonly FieldRules Empty = new FieldRules(EmptyRules);

        public FieldRules(params FieldRule[] fields)
            : base(fields?.ToList() ?? EmptyRules)
        {
        }

        public FieldRules(IList<FieldRule> list)
            : base(list ?? EmptyRules)
        {
        }
    }
}
