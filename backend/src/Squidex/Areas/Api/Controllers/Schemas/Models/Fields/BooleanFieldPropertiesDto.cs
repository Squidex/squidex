// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

[OpenApiRequest]
public sealed class BooleanFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The language specific default value for the field value.
    /// </summary>
    public LocalizedValue<bool?>? DefaultValues { get; set; }

    /// <summary>
    /// The default value for the field value.
    /// </summary>
    public bool? DefaultValue { get; set; }

    /// <summary>
    /// Indicates that the inline editor is enabled for this field.
    /// </summary>
    public bool InlineEditable { get; set; }

    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public BooleanFieldEditor Editor { get; set; }

    public static BooleanFieldPropertiesDto FromDomain(BooleanFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new BooleanFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new BooleanFieldProperties());
    }
}
