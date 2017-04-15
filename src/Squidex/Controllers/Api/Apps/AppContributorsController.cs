// ==========================================================================
//  AppContributorsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Core.Identity;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [Authorize(Roles = SquidexRoles.AppOwner)]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Apps")]
    public class AppContributorsController : ControllerBase
    {
        private readonly IAppProvider appProvider;

        public AppContributorsController(ICommandBus commandBus, IAppProvider appProvider) 
            : base(commandBus)
        {
            this.appProvider = appProvider;
        }

        /// <summary>
        /// Get app contributors.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App contributors returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ContributorDto[]), 200)]
        public async Task<IActionResult> GetContributors(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var response = entity.Contributors.Select(x => SimpleMapper.Map(x, new ContributorDto())).ToList();

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Assign contributor to app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Contributor object that needs to be added to the app.</param>
        /// <returns>
        /// 204 => User assigned to app.
        /// 400 => User is already assigned to the app or not found.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostContributor(string app, [FromBody] AssignAppContributorDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new AssignContributor()));

            return NoContent();
        }

        /// <summary>
        /// Remove contributor from app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the contributor.</param>
        /// <returns>
        /// 204 => User removed from app.
        /// 400 => User is not assigned to the app.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/contributors/{id}/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> DeleteContributor(string app, string id)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = id });

            return NoContent();
        }
    }
}
