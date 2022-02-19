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
        public FieldEnumType(string name, IEnumerable<T> values)
        {
            Name = name;

            var names = new Names();

            foreach (var value in values)
            {
                if (!Equals(value, null))
                {
                    AddValue(names[value.ToString()!.Slugify().ToPascalCase()], null, value);
                }
            }
        }
    }
}
