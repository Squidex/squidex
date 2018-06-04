// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class JTokenValue : ValueNode<JToken>
    {
        public JTokenValue(JToken value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<JToken> node)
        {
            return node.Value.Equals(Value);
        }
    }
}
