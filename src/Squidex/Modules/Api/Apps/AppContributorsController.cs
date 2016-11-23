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
    [Authorize(Roles = "app-owner")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
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
        [HttpGet]
        [Route("apps/{app}/contributors/")]
        [SwaggerTags("Apps")]
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
        [HttpPost]
        [Route("apps/{app}/contributors/")]
        [SwaggerTags("Apps")]
        [DescribedResponseType(400, typeof(ErrorDto), "User is already assigned to the app or not found.")]
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
        [HttpDelete]
        [Route("apps/{app}/contributors/{contributorId}/")]
        [SwaggerTags("Apps")]
        [DescribedResponseType(400, typeof(ErrorDto), "User is not assigned to the app.")]
        public async Task<IActionResult> DeleteContributor(string app, string contributorId)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = contributorId });

            return Ok();
        }
    }
}
