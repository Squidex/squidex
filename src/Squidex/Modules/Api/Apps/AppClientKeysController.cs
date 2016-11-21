// ==========================================================================
//  AppClientKeysController.cs
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
using Squidex.Write.Apps;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
{
    [Authorize(Roles = "app-owner")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    public class AppClientKeysController : ControllerBase
    {
        private readonly IAppProvider appProvider;
        private readonly ClientKeyGenerator keyGenerator;

        public AppClientKeysController(ICommandBus commandBus, IAppProvider appProvider, ClientKeyGenerator keyGenerator)
            : base(commandBus)
        {
            this.appProvider = appProvider;
            this.keyGenerator = keyGenerator;
        }

        [HttpGet]
        [Route("apps/{app}/client-keys/")]
        public async Task<IActionResult> GetContributors(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.ClientKeys.Select(x => SimpleMapper.Map(x, new ClientKeyDto())).ToList();

            return Ok(model);
        }

        [HttpPost]
        [Route("apps/{app}/client-keys/")]
        public async Task<IActionResult> PostClientKey()
        {
            var clientKey = keyGenerator.GenerateKey();

            await CommandBus.PublishAsync(new CreateClientKey { ClientKey = clientKey });

            return Ok(new ClientKeyCreatedDto { ClientKey = clientKey });
        }
    }
}
