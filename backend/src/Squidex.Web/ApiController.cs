// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web
{
    [Area("api")]
    [ApiController]
    [ApiExceptionFilter]
    [ApiModelValidation(false)]
    [Route(Constants.PrefixApi)]
    public abstract class ApiController : Controller
    {
        private readonly Lazy<Resources> resources;

        protected ICommandBus CommandBus { get; }

        protected IAppEntity App
        {
            get
            {
                var app = HttpContext.Features.Get<IAppFeature>()?.App;

                if (app == null)
                {
                    ThrowHelper.InvalidOperationException("Not in a app context.");
                    return default!;
                }

                return app;
            }
        }

        protected ISchemaEntity Schema
        {
            get
            {
                var schema = HttpContext.Features.Get<ISchemaFeature>()?.Schema;

                if (schema == null)
                {
                    ThrowHelper.InvalidOperationException("Not in a schema context.");
                    return default!;
                }

                return schema;
            }
        }

        protected Resources Resources
        {
            get => resources.Value;
        }

        protected Context Context
        {
            get => HttpContext.Context();
        }

        protected DomainId AppId
        {
            get => App.Id;
        }

        protected ApiController(ICommandBus commandBus)
        {
            CommandBus = commandBus;

            resources = new Lazy<Resources>(() => new Resources(this));
        }
    }
}
