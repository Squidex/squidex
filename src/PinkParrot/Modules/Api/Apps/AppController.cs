using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Modules.Api.Apps.Models;
using PinkParrot.Modules.Api.Schemas.Models;
using PinkParrot.Read.Apps.Repositories;

namespace PinkParrot.Modules.Api.Apps
{
    public class AppController
    {
        private readonly IAppRepository appRepository;

        public AppController(IAppRepository appRepository)
        {
            this.appRepository = appRepository;
        }

        [HttpGet]
        [Route("api/schemas/")]
        public async Task<List<ListAppDto>> Query()
        {
            var schemas = await appRepository.QueryAllAsync();

            return schemas.Select(s => SimpleMapper.Map(s, new ListAppDto())).ToList();
        }
    }
}
