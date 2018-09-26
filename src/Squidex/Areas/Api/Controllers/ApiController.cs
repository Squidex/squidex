// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers
{
    [Area("Api")]
    [ApiModelValidation(false)]
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

        protected Guid AppId
        {
            get { return App.Id; }
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
