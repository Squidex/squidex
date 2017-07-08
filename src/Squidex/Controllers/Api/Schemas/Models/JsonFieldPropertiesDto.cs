// ==========================================================================
//  JsonFieldPropertiesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("Json")]
    public sealed class JsonFieldPropertiesDto : FieldPropertiesDto
    {
        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new JsonFieldProperties());
        }
    }
}
