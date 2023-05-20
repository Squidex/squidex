// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

[OpenApiRequest]
public sealed class ComponentFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The ID of the embedded schemas.
    /// </summary>
    public ReadonlyList<DomainId>? SchemaIds { get; set; }

    public static ComponentFieldPropertiesDto FromDomain(ComponentFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new ComponentFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new ComponentFieldProperties());
    }
}
