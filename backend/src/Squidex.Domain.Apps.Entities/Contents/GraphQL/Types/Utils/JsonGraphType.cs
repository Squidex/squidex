// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class JsonGraphType : ScalarGraphType
    {
        public JsonGraphType()
        {
            Name = "JsonScalar";

            Description = "Unstructured Json object";
        }

        public override object Serialize(object value)
        {
            return value;
        }

        public override object ParseValue(object value)
        {
            return JsonConverter.ParseJson(value);
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is JsonValueNode jsonGraphType)
            {
                return jsonGraphType.Value;
            }

            return value;
        }
    }
}
