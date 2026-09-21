// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Export;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

[OpenApiRequest]
public sealed class ExportContentsDto
{
    /// <summary>
    /// The format of the exported file. Default: Csv.
    /// </summary>
    public ExportFormat? Format { get; set; }

    /// <summary>
    /// The optional comma separated list of fields in the format 'name=path', for example 'id,Title=data.title.en'.
    /// </summary>
    public string? Fields { get; set; }

    /// <summary>
    /// True, to export the unpublished versions. Default: false.
    /// </summary>
    public bool? Unpublished { get; set; }
}
