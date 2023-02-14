// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

public sealed class GeolocationFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public GeolocationFieldEditor Editor { get; set; }

    public static GeolocationFieldPropertiesDto FromDomain(GeolocationFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new GeolocationFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new GeolocationFieldProperties());
    }
}
