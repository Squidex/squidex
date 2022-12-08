// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using GraphQL.Utilities;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

public sealed class FieldEnumType : EnumerationGraphType
{
    public FieldEnumType(string name, IEnumerable<string> values)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = name;

        foreach (var value in values)
        {
            Add(value, value, value);
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
