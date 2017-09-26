// ==========================================================================
//  JsonFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
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

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            yield break;
        }
    }
}
