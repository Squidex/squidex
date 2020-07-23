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
    public sealed class FieldNames : ReadOnlyCollection<string>
    {
        private static readonly List<string> EmptyNames = new List<string>();

        public static readonly FieldNames Empty = new FieldNames(EmptyNames);

        public FieldNames(params string[] fields)
            : base(fields?.ToList() ?? EmptyNames)
        {
        }

        public FieldNames(IList<string> list)
            : base(list ?? EmptyNames)
        {
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
