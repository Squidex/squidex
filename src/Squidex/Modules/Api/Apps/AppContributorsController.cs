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
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Apps.Models;
using Squidex.Pipeline;
using Squidex.Read.Apps.Repositories;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
{
    [Authorize]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [Route("apps/{app}")]
    public class AppContributorsController : ControllerBase
    {
        private readonly IAppRepository appRepository;

        public AppContributorsController(ICommandBus commandBus, IAppRepository appRepository) 
            : base(commandBus)
        {
            this.appRepository = appRepository;
        }

        [HttpGet]
        [Route("contributors")]
        public async Task<IActionResult> GetContributors(string app)
        {
            var entity = await appRepository.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.Contributors.Select(x => SimpleMapper.Map(x, new AppContributorDto())).ToList();

            return Ok(model);
        }

        [HttpPut]
        [Route("contributors")]
        public async Task<IActionResult> PutContributor([FromBody] AssignContributorDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new AssignContributor()));

            return Ok();
        }

        [HttpDelete]
        [Route("contributors/{contributorId}")]
        public async Task<IActionResult> PutContributor(string contributorId)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = contributorId });

            return Ok();
        }
    }
}
