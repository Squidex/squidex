// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Web;

[Area("api")]
[ApiController]
[ApiExceptionFilter]
[ApiModelValidation(false)]
[Route(Constants.PrefixApi)]
public abstract class ApiController : Controller
{
    private readonly Lazy<Resources> resources;

    protected ICommandBus CommandBus { get; }

    protected App App
    {
        get
        {
            var app = HttpContext.Features.Get<App>();

            if (app == null)
            {
                ThrowHelper.InvalidOperationException("Not in a app context.");
                return default!;
            }

            return app;
        }
    }

    protected Team Team
    {
        get
        {
            var team = HttpContext.Features.Get<Team>();

            if (team == null)
            {
                ThrowHelper.InvalidOperationException("Not in a team context.");
                return default!;
            }

            return team;
        }
    }

    protected Schema Schema
    {
        get
        {
            var schema = HttpContext.Features.Get<Schema>();

            if (schema == null)
            {
                ThrowHelper.InvalidOperationException("Not in a schema context.");
                return default!;
            }

            return schema;
        }
    }

    protected string UserId
    {
        get
        {
            var subject = User.OpenIdSubject();

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new DomainForbiddenException(T.Get("common.httpOnlyAsUser"));
            }

            return subject;
        }
    }

    protected bool IsFrontend
    {
        get => HttpContext.User.IsInClient(DefaultClients.Frontend);
    }

    protected string UserOrClientId
    {
        get => HttpContext.User.UserOrClientId()!;
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

    protected DomainId TeamId
    {
        get => Team.Id;
    }

    protected ApiController(ICommandBus commandBus)
    {
        CommandBus = commandBus;

        resources = new Lazy<Resources>(() => new Resources(this));
    }
}
