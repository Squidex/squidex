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
    public sealed class JsonValue : ValueNode<JObject>
    {
        public JsonValue(JObject value)
        {
            Value = value;
        }

        protected override bool Equals(ValueNode<JObject> node)
        {
            return false;
        }
    }
}
