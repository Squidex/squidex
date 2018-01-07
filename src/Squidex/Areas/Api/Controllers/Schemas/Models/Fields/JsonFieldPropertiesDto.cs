// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
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
