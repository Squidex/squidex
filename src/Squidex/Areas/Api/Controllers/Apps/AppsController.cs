// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [SwaggerTag(nameof(Apps))]
    public sealed class AppsController : ApiController
    {
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlansProvider;

        public AppsController(ICommandBus commandBus,
            IAppProvider appProvider,
            IAppPlansProvider appPlansProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
            this.appPlansProvider = appPlansProvider;
        }

        /// <summary>
        /// Get your apps.
        /// </summary>
        /// <returns>
        /// 200 => Apps returned.
        /// </returns>
        /// <remarks>
        /// You can only retrieve the list of apps when you are authenticated as a user (OpenID implicit flow).
        /// You will retrieve all apps, where you are assigned as a contributor.
        /// </remarks>
        [HttpGet]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppDto[]), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetApps()
        {
            var subject = HttpContext.User.OpenIdSubject();

            var apps = await appProvider.GetUserApps(subject);

            var response = apps.Select(a =>
            {
                var dto = SimpleMapper.Map(a, new AppDto());

                dto.Permission = a.Contributors[subject];

                dto.PlanName = appPlansProvider.GetPlanForApp(a)?.Name;
                dto.PlanUpgrade = appPlansProvider.GetPlanUpgradeForApp(a)?.Name;

                return dto;
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app.
        /// </summary>
        /// <param name="request">The app object that needs to be added to squidex.</param>
        /// <returns>
        /// 201 => App created.
        /// 400 => App object is not valid.
        /// 409 => App name is already in use.
        /// </returns>
        /// <remarks>
        /// You can only create an app when you are authenticated as a user (OpenID implicit flow).
        /// You will be assigned as owner of the new app automatically.
        /// </remarks>
        [HttpPost]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostApp([FromBody] CreateAppDto request)
        {
            var command = SimpleMapper.Map(request, new CreateApp());

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = new AppCreatedDto { Id = result.IdOrValue.ToString(), Version = result.Version };

            response.Permission = AppContributorPermission.Owner;

            response.PlanName = appPlansProvider.GetPlan(null)?.Name;
            response.PlanUpgrade = appPlansProvider.GetPlanUpgrade(null)?.Name;

            return CreatedAtAction(nameof(GetApps), response);
        }
    }
}
