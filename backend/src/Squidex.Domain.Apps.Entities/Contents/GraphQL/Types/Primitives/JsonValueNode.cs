// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQLParser.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

internal sealed class JsonValueNode : GraphQLValue
{
    public override ASTNodeKind Kind => ASTNodeKind.ObjectValue;

    public JsonValue Value { get; }

    public JsonValueNode(JsonValue value)
    {
        Value = value;
    }
}
