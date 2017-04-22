// ==========================================================================
//  AssetsFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("AssetsField")]
    public sealed class AssetsFieldProperties : FieldProperties
    {
        public override JToken GetDefaultValue()
        {
            return new JArray();
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            yield break;
        }
    }
}
