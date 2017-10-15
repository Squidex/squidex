// ==========================================================================
//  JsonFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(JsonField))]
    public sealed class JsonFieldProperties : FieldProperties
    {
        public override JToken GetDefaultValue()
        {
            return JValue.CreateNull();
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
