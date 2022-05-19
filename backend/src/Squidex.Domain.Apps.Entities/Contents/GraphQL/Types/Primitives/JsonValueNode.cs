// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQLParser;
using GraphQLParser.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal sealed class JsonValueNode : GraphQLValue
    {
        public override ASTNodeKind Kind => ASTNodeKind.ObjectValue;

        public IJsonValue Value { get; }

        public JsonValueNode(IJsonValue value)
        {
            Value = value;
        }
    }
}
