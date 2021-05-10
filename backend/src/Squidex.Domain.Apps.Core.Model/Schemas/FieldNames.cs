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
    public sealed class FieldNames : ImmutableList<string>
    {
        public static readonly FieldNames Empty = new FieldNames(new List<string>());

        public FieldNames()
        {
        }

        public FieldNames(IList<string> list)
            : base(list)
        {
        }

        public static FieldNames Create(params string[] names)
        {
            return new FieldNames(names.ToList());
        }

        public FieldNames Add(string field)
        {
            var list = this.ToList();

            list.Add(field);

            return new FieldNames(list);
        }

        public FieldNames Remove(string field)
        {
            var list = this.ToList();

            list.Remove(field);

            return new FieldNames(list);
        }
    }
}
