// ==========================================================================
//  ControllerBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public ICommandBus CommandBus { get; }

        protected ControllerBase(ICommandBus commandBus)
        {
            CommandBus = commandBus;
        }

        public Guid AppId
        {
            get
            {
                var appFeature = HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Not in a app context");
                }

                return appFeature.App.Id;
            }
        }
    }
}
