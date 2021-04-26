// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Docs
{
    public sealed class DocsController : ApiController
    {
        public DocsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        [HttpGet]
        [Route("docs/")]
        public IActionResult Docs()
        {
            var vm = new DocsVM
            {
                Specification = "~/swagger/v1/swagger.json"
            };

            return View(nameof(Docs), vm);
        }
    }
}
