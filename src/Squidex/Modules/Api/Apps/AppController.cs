// ==========================================================================
//  AppController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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
    [DeactivateForAppDomain]
    public class AppController : ControllerBase
    {
        private readonly IAppRepository appRepository;

        public AppController(ICommandBus commandBus, IAppRepository appRepository) 
            : base(commandBus)
        {
            this.appRepository = appRepository;
        }

        [HttpGet]
        [Route("api/apps/")]
        public async Task<List<ListAppDto>> Query()
        {
            var schemas = await appRepository.QueryAllAsync();

            return schemas.Select(s => SimpleMapper.Map(s, new ListAppDto())).ToList();
        }

        [HttpPost]
        [Route("api/apps/")]
        public async Task<IActionResult> Create([FromBody] CreateAppDto model)
        {
            var command = SimpleMapper.Map(model, new CreateApp { AggregateId = Guid.NewGuid() });

            await CommandBus.PublishAsync(command);

            return CreatedAtAction("Query", new EntityCreatedDto { Id = command.AggregateId });
        }
    }
}
