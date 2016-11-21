// ==========================================================================
//  AppLanguagesController.cs
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
    [Authorize(Roles = "app-owner")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    public class AppLanguagesController : ControllerBase
    {
        private readonly IAppProvider appProvider;

        public AppLanguagesController(ICommandBus commandBus, IAppProvider appProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
        }

        [HttpGet]
        [Route("apps/{app}/languages/")]
        public async Task<IActionResult> GetContributors(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.Languages.Select(x => SimpleMapper.Map(x, new LanguageDto())).ToList();

            return Ok(model);
        }

        [HttpPost]
        [Route("apps/{app}/languages/")]
        public async Task<IActionResult> PostLanguages([FromBody] ConfigureLanguagesDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new ConfigureLanguages()));

            return Ok();
        }
    }
}
