// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class InstantValueNode : ValueNode<Instant>
    {
        public InstantValueNode(Instant value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<Instant> node)
        {
            return Equals(Value, node.Value);
        }
    }
}
