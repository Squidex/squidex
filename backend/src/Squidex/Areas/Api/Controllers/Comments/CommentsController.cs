// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;
using YDotNet.Server.WebSockets;

namespace Squidex.Areas.Api.Controllers.Comments;

public sealed class CommentsController : ApiController
{
    private readonly ICollaborationService collaboration;

    public CommentsController(ICommandBus commandBus, ICollaborationService collaboration)
        : base(commandBus)
    {
        this.collaboration = collaboration;
    }

    [Route("users/collaboration")]
    [ApiPermission]
    public IActionResult UserDocument()
    {
        return new YDotNetActionResult(collaboration.UserDocument(User.UserOrClientId()!));
    }

    [Route("apps/{app}/collaboration/{commentsId}")]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsRead)]
    [ApiCosts(0)]
    public IActionResult CollaborationDocument(string app, DomainId commentsId)
    {
        return new YDotNetActionResult(collaboration.ResourceDocument(App.NamedId(), commentsId));
    }
}
