// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Extensions.Samples.Controllers;

public sealed class PluginController : ApiController
{
    public PluginController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    [Route("plugins/sample")]
    public IActionResult Test()
    {
        return Ok(new { text = "I am Plugin" });
    }
}
