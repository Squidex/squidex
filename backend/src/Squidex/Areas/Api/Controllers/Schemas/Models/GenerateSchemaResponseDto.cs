// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class GenerateSchemaResponseDto
{
    /// <summary>
    /// The status log.
    /// </summary>
    public ReadonlyList<string> Log { get; set; } = [];

    /// <summary>
    /// The name of the created schema.
    /// </summary>
    public string? SchemaName { get; set; }

    public static GenerateSchemaResponseDto FromDomain(SchemaAIResult response)
    {
        return SimpleMapper.Map(response, new GenerateSchemaResponseDto());
    }
}
