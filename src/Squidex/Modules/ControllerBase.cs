// ==========================================================================
//  ControllerBase.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Pipeline;

namespace PinkParrot.Modules
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

                return appFeature.AppId;
            }
        }
    }
}
