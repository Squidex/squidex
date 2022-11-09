// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class UpsertSchemaFieldDto
{
    /// <summary>
    /// The name of the field. Must be unique within the schema.
    /// </summary>
    [LocalizedRequired]
    [LocalizedRegularExpression("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$")]
    public string Name { get; set; }

    /// <summary>
    /// Defines if the field is hidden.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Defines if the field is locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Defines if the field is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Determines the optional partitioning of the field.
    /// </summary>
    public string? Partitioning { get; set; }

    /// <summary>
    /// The field properties.
    /// </summary>
    [LocalizedRequired]
    public FieldPropertiesDto Properties { get; set; }

    /// <summary>
    /// The nested fields.
    /// </summary>
    public UpsertSchemaNestedFieldDto[]? Nested { get; set; }
}