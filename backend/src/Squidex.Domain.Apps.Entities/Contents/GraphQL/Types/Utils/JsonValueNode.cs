﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class JsonValueNode : ValueNode<IJsonValue>
    {
        public JsonValueNode(IJsonValue value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<IJsonValue> node)
        {
            return false;
        }
    }
}
