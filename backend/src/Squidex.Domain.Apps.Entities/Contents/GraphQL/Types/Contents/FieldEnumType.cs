// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using GraphQL.Utilities;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public sealed class FieldEnumType : EnumerationGraphType
    {
        public FieldEnumType(string name, IEnumerable<string> values)
        {
            Name = name;

            foreach (var value in values)
            {
                AddValue(value, null, value);
            }
        }

        public static FieldEnumType? TryCreate(string name, IEnumerable<string> values)
        {
            if (!values.All(x => x.IsValidName(NamedElement.EnumValue)) || !values.Any())
            {
                return null;
            }

            return new FieldEnumType(name, values);
        }
    }
}
