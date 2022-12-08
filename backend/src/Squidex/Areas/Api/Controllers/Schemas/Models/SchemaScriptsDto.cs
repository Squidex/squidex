// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class SchemaScriptsDto
{
    /// <summary>
    /// The script that is executed for each content when querying contents.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// The script that is executed for all contents when querying contents.
    /// </summary>
    public string? QueryPre { get; set; }

    /// <summary>
    /// The script that is executed when creating a content.
    /// </summary>
    public string? Create { get; set; }

    /// <summary>
    /// The script that is executed when updating a content.
    /// </summary>
    public string? Update { get; set; }

    /// <summary>
    /// The script that is executed when deleting a content.
    /// </summary>
    public string? Delete { get; set; }

    /// <summary>
    /// The script that is executed when change a content status.
    /// </summary>
    public string? Change { get; set; }

    public ConfigureScripts ToCommand()
    {
        var scripts = SimpleMapper.Map(this, new SchemaScripts());

        return new ConfigureScripts { Scripts = scripts };
    }
}
