// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class NoopGraphType : ScalarGraphType
    {
        public NoopGraphType(string name)
        {
            Name = name;
        }

        public NoopGraphType(IGraphType type)
            : this(type.Name)
        {
            Description = type.Description;
        }

        public override object Serialize(object value)
        {
            return value;
        }

        public override object ParseValue(object value)
        {
            return value;
        }

        public override object ParseLiteral(IValue value)
        {
            return value.Value;
        }
    }
}
