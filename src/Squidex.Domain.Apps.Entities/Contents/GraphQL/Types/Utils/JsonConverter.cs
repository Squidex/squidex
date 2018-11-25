// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class JsonConverter : IAstFromValueConverter
    {
        public static readonly JsonConverter Instance = new JsonConverter();

        private JsonConverter()
        {
        }

        public IValue Convert(object value, IGraphType type)
        {
            return new JsonValue(value as JsonObject);
        }

        public bool Matches(object value, IGraphType type)
        {
            return value is JsonObject;
        }
    }
}
