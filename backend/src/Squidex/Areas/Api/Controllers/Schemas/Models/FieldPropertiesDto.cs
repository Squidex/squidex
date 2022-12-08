// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public abstract class FieldPropertiesDto
{
    /// <summary>
    /// Optional label for the editor.
    /// </summary>
    [LocalizedStringLength(100)]
    public string? Label { get; set; }

    /// <summary>
    /// Hints to describe the field.
    /// </summary>
    [LocalizedStringLength(1000)]
    public string? Hints { get; set; }

    /// <summary>
    /// Placeholder to show when no value has been entered.
    /// </summary>
    [LocalizedStringLength(100)]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Indicates if the field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates if the field is required when publishing.
    /// </summary>
    public bool IsRequiredOnPublish { get; set; }

    /// <summary>
    /// Indicates if the field should be rendered with half width only.
    /// </summary>
    public bool IsHalfWidth { get; set; }

    /// <summary>
    /// Optional url to the editor.
    /// </summary>
    public string? EditorUrl { get; set; }

    /// <summary>
    /// Tags for automation processes.
    /// </summary>
    public ReadonlyList<string>? Tags { get; set; }

    public abstract FieldProperties ToProperties();
}
