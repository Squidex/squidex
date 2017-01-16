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
using Squidex.Read.Apps;

namespace Squidex.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public ICommandBus CommandBus { get; }

        protected ControllerBase(ICommandBus commandBus)
        {
            CommandBus = commandBus;
        }

        protected ControllerBase()
        {
            throw new NotImplementedException();
        }

        public IAppEntity App
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

        public Guid AppId
        {
            get
            {
                return App.Id;
            }
        }
    }
}
