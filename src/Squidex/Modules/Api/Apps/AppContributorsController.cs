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
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
{
    [Authorize]
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

        [HttpGet]
        [Route("apps/{app}/contributors/")]
        public async Task<IActionResult> GetContributors(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.Contributors.Select(x => SimpleMapper.Map(x, new AppContributorDto())).ToList();

            return Ok(model);
        }

        [HttpPost]
        [Route("apps/{app}/contributors/")]
        public async Task<IActionResult> PostContributor([FromBody] AssignContributorDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new AssignContributor()));

            return Ok();
        }

        [HttpDelete]
        [Route("apps/{app}/contributors/{contributorId}/")]
        public async Task<IActionResult> PutContributor(string contributorId)
        {
            await CommandBus.PublishAsync(new RemoveContributor { ContributorId = contributorId });

            return Ok();
        }
    }
}
