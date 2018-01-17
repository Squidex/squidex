// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class GuidGraphType : ScalarGraphType
    {
        public GuidGraphType()
        {
            Name = "Guid";

            Description = "The `Guid` scalar type global unique identifier";
        }

        public override object Serialize(object value)
        {
            return ParseValue(value)?.ToString();
        }

        public override object ParseValue(object value)
        {
            if (value is Guid guid)
            {
                return guid;
            }

            var inputValue = value?.ToString().Trim('"');

            if (Guid.TryParse(inputValue, out guid))
            {
                return guid;
            }

            return null;
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue stringValue)
            {
                return ParseValue(stringValue.Value);
            }

            return null;
        }
    }
}
