// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class GenerateSchemaDto
{
    /// <summary>
    /// The prompt to generate.
    /// </summary>
    [LocalizedRequired]
    public string Prompt { get; set; }

    /// <summary>
    /// Indicates if the schema should actually be generated.
    /// </summary>
    public bool Execute { get; set; }

    /// <summary>
    /// The number of content items to generate.
    /// </summary>
    public int NumberOfContentItems { get; set; }
}
