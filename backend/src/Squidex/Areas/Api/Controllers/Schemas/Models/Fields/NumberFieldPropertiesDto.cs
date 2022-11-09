// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

public sealed class NumberFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The language specific default value for the field value.
    /// </summary>
    public LocalizedValue<double?>? DefaultValues { get; set; }

    /// <summary>
    /// The default value for the field value.
    /// </summary>
    public double? DefaultValue { get; set; }

    /// <summary>
    /// The maximum allowed value for the field value.
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// The minimum allowed value for the field value.
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// The allowed values for the field value.
    /// </summary>
    public ReadonlyList<double>? AllowedValues { get; set; }

    /// <summary>
    /// Indicates if the field value must be unique. Ignored for nested fields and localized fields.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Indicates that the inline editor is enabled for this field.
    /// </summary>
    public bool InlineEditable { get; set; }

    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public NumberFieldEditor Editor { get; set; }

    public override FieldProperties ToProperties()
    {
        var result = SimpleMapper.Map(this, new NumberFieldProperties());

        return result;
    }
}
