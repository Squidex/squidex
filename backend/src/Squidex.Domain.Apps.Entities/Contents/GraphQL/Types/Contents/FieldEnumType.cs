// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public sealed class FieldEnumType<T> : EnumerationGraphType
    {
        public FieldEnumType(string name, string prefix, IEnumerable<T> values)
        {
            Name = name;

            var index = 0;

            foreach (var value in values)
            {
                AddValue(BuildName(value, prefix, index), null, value);

                index++;
            }
        }

        private static string BuildName(T value, string prefix, int index)
        {
            var name = value!.ToString()!.Slugify().ToPascalCase();

            if (string.IsNullOrEmpty(name))
            {
                name = $"{prefix}_{index}";
            }

            if (!char.IsLetter(name[0]))
            {
                name = $"{prefix}_{name}";
            }

            return name;
        }
    }
}
