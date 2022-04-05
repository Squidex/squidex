// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    public class JsonNoopGraphType : ScalarGraphType
    {
        public JsonNoopGraphType()
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = "JsonScalar";

            Description = "Unstructured Json object";
        }

        public override object? ParseLiteral(IValue value)
        {
            return value.Value;
        }

        public override object? ParseValue(object? value)
        {
            return value;
        }

        public override object? Serialize(object? value)
        {
            return value;
        }
    }
}
