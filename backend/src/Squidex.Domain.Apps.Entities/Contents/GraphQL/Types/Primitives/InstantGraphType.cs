// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;
using NodaTime.Text;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
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

        public override object? ParseLiteral(IValue value)
        {
            switch (value)
            {
                case StringValue stringValue:
                    return ParseValue(stringValue.Value);
                default:
                    return null;
            }
        }
    }
}
