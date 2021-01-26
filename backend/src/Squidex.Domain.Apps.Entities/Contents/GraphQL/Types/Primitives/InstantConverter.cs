// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal sealed class InstantConverter : IAstFromValueConverter
    {
        public static readonly InstantConverter Instance = new InstantConverter();

        private InstantConverter()
        {
        }

        public IValue Convert(object value, IGraphType type)
        {
            return new InstantValueNode((Instant)value);
        }

        public bool Matches(object value, IGraphType type)
        {
            return type is InstantGraphType;
        }
    }
}
