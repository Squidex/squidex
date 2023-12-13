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
public sealed class RichTextFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The initial id to the folder when the control supports file uploads.
    /// </summary>
    public string? FolderId { get; set; }

    /// <summary>
    /// The minimum allowed length for the field value.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// The maximum allowed length for the field value.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// The minimum allowed of normal characters for the field value.
    /// </summary>
    public int? MinCharacters { get; set; }

    /// <summary>
    /// The maximum allowed of normal characters for the field value.
    /// </summary>
    public int? MaxCharacters { get; set; }

    /// <summary>
    /// The minimum allowed number of words for the field value.
    /// </summary>
    public int? MinWords { get; set; }

    /// <summary>
    /// The maximum allowed number of words for the field value.
    /// </summary>
    public int? MaxWords { get; set; }

    /// <summary>
    /// The class names for the editor.
    /// </summary>
    public ReadonlyList<string>? ClassNames { get; set; }

    /// <summary>
    /// The allowed schema ids that can be embedded.
    /// </summary>
    public ReadonlyList<DomainId>? SchemaIds { get; init; }

    public static RichTextFieldPropertiesDto FromDomain(RichTextFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new RichTextFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new RichTextFieldProperties());
    }
}
