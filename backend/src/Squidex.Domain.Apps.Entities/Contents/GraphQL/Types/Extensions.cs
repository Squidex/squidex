// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class Extensions
    {
        public static IEnumerable<(T Field, string Name, string Type)> SafeFields<T>(this IEnumerable<T> fields) where T : IField
        {
            var allFields =
                fields.FieldNames()
                    .GroupBy(x => x.Name)
                    .Select(g => g.Select((f, i) => (f.Field, f.Name.SafeString(i), f.Type.SafeString(i))))
                    .SelectMany(x => x);

            return allFields;
        }

        private static IEnumerable<(T Field, string Name, string Type)> FieldNames<T>(this IEnumerable<T> fields) where T : IField
        {
            return fields.ForApi().Select(field => (field, field.Name.ToCamelCase(), field.TypeName()));
        }

        private static string SafeString(this string value, int index)
        {
            if (value.Length > 0 && !char.IsLetter(value[0]))
            {
                value = "gql_" + value;
            }

            if (index > 0)
            {
                return value + (index + 1);
            }

            return value;
        }
    }
}
