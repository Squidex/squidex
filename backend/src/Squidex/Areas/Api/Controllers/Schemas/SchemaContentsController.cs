// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Contents.Migration;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas;

/// <summary>
/// Update and query information about schemas.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Schemas))]
[ApiModelValidation(true)]
public class SchemaContentsController(ICommandBus commandBus, IJobService jobService) : ApiController(commandBus)
{
    /// <summary>
    /// Migrate the contents to the current schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <response code="204">Content migration added to job queue.</response>
    /// <response code="404">Schema or app not found.</response>
    /// <remarks>
    /// Schema changes are not applied to the stored contents. This endpoint starts a job that rewrites all
    /// contents of the schema so that the stored data matches the current schema.
    /// </remarks>
    [HttpPost]
    [Route("apps/{app}/schemas/{schema}/contents/migrate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasMigrate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostContentMigration(string app, string schema)
    {
        var job = MigrateContentsJob.BuildRequest(User.Token()!, App, Schema);

        await jobService.StartAsync(App.Id, job, HttpContext.RequestAborted);

        return NoContent();
    }
}
