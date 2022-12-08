// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class RuleElementPropertyDto
{
    /// <summary>
    /// The html editor.
    /// </summary>
    [LocalizedRequired]
    public RuleFieldEditor Editor { get; set; }

    /// <summary>
    /// The name of the editor.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The label to use.
    /// </summary>
    [LocalizedRequired]
    public string Display { get; set; }

    /// <summary>
    /// The options, if the editor is a dropdown.
    /// </summary>
    public string[]? Options { get; set; }

    /// <summary>
    /// The optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if the property is formattable.
    /// </summary>
    public bool IsFormattable { get; set; }

    /// <summary>
    /// Indicates if the property is required.
    /// </summary>
    public bool IsRequired { get; set; }
}
