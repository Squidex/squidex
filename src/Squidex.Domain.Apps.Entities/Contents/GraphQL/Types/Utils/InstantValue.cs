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
    public sealed class InstantValue : ValueNode<Instant>
    {
        public InstantValue(Instant value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<Instant> node)
        {
            return Value.Equals(node.Value);
        }
    }
}
