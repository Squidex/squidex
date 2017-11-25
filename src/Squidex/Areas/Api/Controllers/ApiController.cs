// ==========================================================================
//  ControllerBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers
{
    [Area("Api")]
    public abstract class ApiController : Controller
    {
        protected ICommandBus CommandBus { get; }

        protected IAppEntity App
        {
            get
            {
                var appFeature = HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Not in a app context.");
                }

                return appFeature.App;
            }
        }

        protected string AppName
        {
            get { return App.Name; }
        }

        protected ApiController(ICommandBus commandBus)
        {
            Guard.NotNull(commandBus, nameof(commandBus));

            CommandBus = commandBus;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.PathBase.StartsWithSegments("/api"))
            {
                context.Result = new RedirectResult("/");
            }
        }
    }
}
