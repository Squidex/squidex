// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class JTokenConverter : IAstFromValueConverter
    {
        public static readonly JTokenConverter Instance = new JTokenConverter();

        private JTokenConverter()
        {
        }

        public IValue Convert(object value, IGraphType type)
        {
            return new JTokenValue(value as JToken);
        }

        public bool Matches(object value, IGraphType type)
        {
            return value is JToken;
        }
    }
}
