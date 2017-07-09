// ==========================================================================
//  GeolocationScalarType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Language.AST;
using GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class GeolocationScalarType : ScalarGraphType
    {
        public GeolocationScalarType()
        {
            Name = "Json";
        }

        public override object Serialize(object value)
        {
            return value;
        }

        public override object ParseValue(object value)
        {
            return value != null ? value is JObject ? value : JObject.FromObject(value) : null;
        }

        public override object ParseLiteral(IValue value)
        {
            return null;
        }
    }
}
