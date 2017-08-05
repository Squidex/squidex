// ==========================================================================
//  AppsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Apps")]
    public class AppsController : ControllerBase
    {
        private readonly IAppRepository appRepository;

        public AppsController(ICommandBus commandBus, IAppRepository appRepository)
            : base(commandBus)
        {
            this.appRepository = appRepository;
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
        [ApiCosts(1)]
        public async Task<IActionResult> GetApps()
        {
            var subject = HttpContext.User.OpenIdSubject();

            var apps = await appRepository.QueryAllAsync(subject);

            var response = apps.Select(s =>
            {
                var dto = SimpleMapper.Map(s, new AppDto());

                dto.Permission = s.Contributors.Single(x => x.ContributorId == subject).Permission;

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
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostApp([FromBody] CreateAppDto request)
        {
            var command = SimpleMapper.Map(request, new CreateApp());

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = new EntityCreatedDto { Id = result.ToString(), Version = result.Version };

            return CreatedAtAction(nameof(GetApps), response);
        }
    }
}
