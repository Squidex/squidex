// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class GraphQLQueryDto
{
    /// <summary>
    /// The optional version of the asset.
    /// </summary>
    [FromQuery(Name = "The query string")]
    public string Query { get; set; }

    /// <summary>
    /// The optional operation variables.
    /// </summary>
    [FromQuery(Name = "variables")]
    public string? Variables { get; set; }

    /// <summary>
    /// The optional operation name.
    /// </summary>
    [FromQuery(Name = "operationName")]
    public string? OperationName { get; set; }
}
