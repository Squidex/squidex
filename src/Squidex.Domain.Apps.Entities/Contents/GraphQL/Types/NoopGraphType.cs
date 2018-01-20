// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NoopGraphType : ScalarGraphType
    {
        public NoopGraphType(string name)
        {
            Name = name;
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
            throw new NotSupportedException();
        }
    }
}
