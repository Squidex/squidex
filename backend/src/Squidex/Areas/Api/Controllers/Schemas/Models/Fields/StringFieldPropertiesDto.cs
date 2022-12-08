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

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

public sealed class StringFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The language specific default value for the field value.
    /// </summary>
    public LocalizedValue<string?>? DefaultValues { get; set; }

    /// <summary>
    /// The default value for the field value.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// The pattern to enforce a specific format for the field value.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// The validation message for the pattern.
    /// </summary>
    public string? PatternMessage { get; set; }

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
    /// The allowed values for the field value.
    /// </summary>
    public ReadonlyList<string>? AllowedValues { get; set; }

    /// <summary>
    /// The allowed schema ids that can be embedded.
    /// </summary>
    public ReadonlyList<DomainId>? SchemaIds { get; init; }

    /// <summary>
    /// Indicates if the field value must be unique. Ignored for nested fields and localized fields.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Indicates that other content items or references are embedded.
    /// </summary>
    public bool IsEmbeddable { get; set; }

    /// <summary>
    /// Indicates that the inline editor is enabled for this field.
    /// </summary>
    public bool InlineEditable { get; set; }

    /// <summary>
    /// Indicates whether GraphQL Enum should be created.
    /// </summary>
    public bool CreateEnum { get; init; }

    /// <summary>
    /// How the string content should be interpreted.
    /// </summary>
    public StringContentType ContentType { get; set; }

    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public StringFieldEditor Editor { get; set; }

    public override FieldProperties ToProperties()
    {
        var result = SimpleMapper.Map(this, new StringFieldProperties());

        return result;
    }
}
