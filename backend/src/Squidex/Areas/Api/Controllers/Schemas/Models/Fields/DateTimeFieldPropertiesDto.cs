// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

[OpenApiRequest]
public sealed class DateTimeFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The language specific default value for the field value.
    /// </summary>
    public LocalizedValue<Instant?>? DefaultValues { get; set; }

    /// <summary>
    /// The default value for the field value.
    /// </summary>
    public Instant? DefaultValue { get; set; }

    /// <summary>
    /// The maximum allowed value for the field value.
    /// </summary>
    public Instant? MaxValue { get; set; }

    /// <summary>
    /// The minimum allowed value for the field value.
    /// </summary>
    public Instant? MinValue { get; set; }

    /// <summary>
    /// The format pattern when displayed in the UI.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public DateTimeFieldEditor Editor { get; set; }

    /// <summary>
    /// The calculated default value for the field value.
    /// </summary>
    public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; set; }

    public static DateTimeFieldPropertiesDto FromDomain(DateTimeFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new DateTimeFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new DateTimeFieldProperties());
    }
}
