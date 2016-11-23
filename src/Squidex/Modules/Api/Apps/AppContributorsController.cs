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
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Apps.Models;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [Authorize(Roles = "app-owner")]
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
        /// Get contributors for the app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App contributors returned.
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

            var model = entity.Contributors.Select(x => SimpleMapper.Map(x, new ContributorDto())).ToList();

            return Ok(model);
        }

        /// <summary>
        /// Assign contributor to the app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="model">Contributor object that needs to be added to the app.</param>
        /// <returns>
        /// 200 => User assigned to app.
        /// 400 => User is already assigned to the app or not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/contributors/")]
        [ProducesResponseType(typeof(ErrorDto[]), 400)]
        public async Task<IActionResult> PostContributor(string app, [FromBody] AssignContributorDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new AssignContributor()));

            return Ok();
        }

        /// <summary>
        /// Removes contributor from app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="contributorId"></param>
        /// <returns>
        /// 200 => User removed from app.
        /// 400 => User is not assigned to the app.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/contributors/{contributorId}/")]
        [ProducesResponseType(typeof(ErrorDto[]), 400)]
        public async Task<IActionResult> DeleteContributor(string app, string contributorId)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = contributorId });

            return Ok();
        }
    }
}
