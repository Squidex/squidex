// ==========================================================================
//  AssetsFieldPropertiesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NJsonSchema.Annotations;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("Assets")]
    public sealed class AssetsFieldPropertiesDto : FieldPropertiesDto
    {
        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new AssetsFieldProperties());
        }
    }
}
