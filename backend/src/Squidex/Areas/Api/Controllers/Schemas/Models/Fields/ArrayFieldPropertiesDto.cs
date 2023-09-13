// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

[OpenApiRequest]
public sealed class ArrayFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The minimum allowed items for the field value.
    /// </summary>
    public int? MinItems { get; set; }

    /// <summary>
    /// The maximum allowed items for the field value.
    /// </summary>
    public int? MaxItems { get; set; }

    /// <summary>
    /// The calculated default value for the field value.
    /// </summary>
    public ArrayCalculatedDefaultValue CalculatedDefaultValue { get; set; }

    /// <summary>
    /// The fields that must be unique.
    /// </summary>
    public ReadonlyList<string>? UniqueFields { get; set; }

    public static ArrayFieldPropertiesDto FromDomain(ArrayFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new ArrayFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new ArrayFieldProperties());
    }
}
