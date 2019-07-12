// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web
{
    [Area("Api")]
    [ApiController]
    [ApiExceptionFilter]
    [ApiModelValidation(false)]
    public abstract class ApiController : Controller
    {
        protected ICommandBus CommandBus { get; }

        protected IAppEntity App
        {
            get
            {
                var app = HttpContext.Context().App;

                if (app == null)
                {
                    throw new InvalidOperationException("Not in a app context.");
                }

                return app;
            }
        }

        protected Context Context
        {
            get { return HttpContext.Context(); }
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
            var request = context.HttpContext.Request;

            if (!request.PathBase.HasValue || !request.PathBase.Value.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectResult("/");
            }
        }
    }
}
