// ==========================================================================
//  ControllerBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected ICommandBus CommandBus { get; }

        protected ControllerBase(ICommandBus commandBus)
        {
            Guard.NotNull(commandBus, nameof(commandBus));

            CommandBus = commandBus;
        }

        protected IAppEntity App
        {
            get
            {
                var appFeature = HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Not in a app context");
                }

                return appFeature.App;
            }
        }

        protected Guid AppId
        {
            get
            {
                return App.Id;
            }
        }
    }
}
