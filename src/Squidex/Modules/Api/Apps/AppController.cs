// ==========================================================================
//  AppController.cs
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
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Modules.Api.Apps.Models;
using Squidex.Pipeline;
using Squidex.Read.Apps.Repositories;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
{
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Apps", Description = "Manages and configures apps.")]
    public class AppController : ControllerBase
    {
        private readonly IAppRepository appRepository;

        public AppController(ICommandBus commandBus, IAppRepository appRepository) 
            : base(commandBus)
        {
            this.appRepository = appRepository;
        }

        /// <summary>
        /// Gets your apps.
        /// </summary>
        /// <remarks>
        /// You can only retrieve the list of apps when you are authenticated as a user (OpenID implicit flow).
        /// You will retrieve all apps, where you are assigned as a contributor.
        /// </remarks>
        [HttpGet]
        [Route("apps/")]
        [SwaggerTags("Apps")]
        [DescribedResponseType(200, typeof(AppDto[]), "Apps returned")]
        public async Task<IActionResult> GetApps()
        {
            var subject = HttpContext.User.OpenIdSubject();
            var schemas = await appRepository.QueryAllAsync(subject);

            var models = schemas.Select(s =>
            {
                var dto = SimpleMapper.Map(s, new AppDto());

                dto.Permission = s.Contributors.Single(x => x.ContributorId == subject).Permission;

                return dto;
            }).ToList();

            return Ok(models);
        }

        /// <summary>
        /// Create a new app.
        /// </summary>
        /// <param name="model">The app object that needs to be added to squided.</param>
        /// <remarks>
        /// You can only create an app when you are authenticated as a user (OpenID implicit flow). 
        /// You will be assigned as owner of the new app automatically.
        /// </remarks>
        [HttpPost]
        [Route("apps/")]
        [SwaggerTags("Apps")]
        [DescribedResponseType(201, typeof(EntityCreatedDto), "App created.")]
        [DescribedResponseType(400, typeof(ErrorDto), "App object is not valid.")]
        [DescribedResponseType(409, typeof(ErrorDto), "App name already in use.")]
        public async Task<IActionResult> PostApp([FromBody] CreateAppDto model)
        {
            var command = SimpleMapper.Map(model, new CreateApp { AggregateId = Guid.NewGuid() });

            await CommandBus.PublishAsync(command);

            return CreatedAtAction(nameof(GetApps), new EntityCreatedDto { Id = command.AggregateId });
        }
    }
}
