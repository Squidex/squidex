// ==========================================================================
//  AppContributorsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [MustBeAppOwner]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Apps))]
    public sealed class AppContributorsController : ControllerBase
    {
        private readonly IAppPlansProvider appPlansProvider;

        public AppContributorsController(ICommandBus commandBus, IAppPlansProvider appPlansProvider)
            : base(commandBus)
        {
            this.appPlansProvider = appPlansProvider;
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
        [ProducesResponseType(typeof(ContributorsDto), 200)]
        [ApiCosts(1)]
        public IActionResult GetContributors(string app)
        {
            var contributors = App.Contributors.Select(x => SimpleMapper.Map(x, new ContributorDto())).ToArray();

            var response = new ContributorsDto { Contributors = contributors, MaxContributors = appPlansProvider.GetPlanForApp(App).MaxContributors };

            Response.Headers["ETag"] = new StringValues(App.Version.ToString());

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
        [ApiCosts(1)]
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
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContributor(string app, string id)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = id });

            return NoContent();
        }
    }
}
