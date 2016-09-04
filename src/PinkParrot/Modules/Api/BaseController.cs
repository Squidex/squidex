// ==========================================================================
//  BaseController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Modules.Api
{
    public abstract class BaseController : Controller
    {
        public ICommandBus CommandBus { get; }

        protected BaseController(ICommandBus commandBus)
        {
            CommandBus = commandBus;
        }
    }
}
