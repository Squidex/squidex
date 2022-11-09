// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using GraphQLParser.AST;
using NodaTime.Text;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

public sealed class InstantGraphType : DateTimeGraphType
{
    public override object? Serialize(object? value)
    {
        return value;
    }

    public override object? ParseValue(object? value)
    {
        return InstantPattern.ExtendedIso.Parse(value?.ToString()!).Value;
    }

    public override object? ParseLiteral(GraphQLValue value)
    {
        switch (value)
        {
            case GraphQLStringValue stringValue:
                return ParseValue(stringValue.Value);
            default:
                return null;
        }
    }
}
