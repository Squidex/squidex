// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;
using YDotNet.Server.WebSockets;

namespace Squidex.Areas.Api.Controllers.Comments;

public sealed class CommentsController : ApiController
{
    private readonly INotificationPublisher notificationPublisher;

    public CommentsController(ICommandBus commandBus, INotificationPublisher notificationPublisher)
        : base(commandBus)
    {
        this.notificationPublisher = notificationPublisher;
    }

    [Route("users/collaboration")]
    [ApiPermission]
    public IActionResult UserDocument()
    {
        return new YDotNetActionResult(notificationPublisher.UserDocument(User.UserOrClientId()!));
    }

    [Route("apps/{app}/collaboration/{commentsId}")]
    [ApiPermissionOrAnonymous(PermissionIds.AppCommentsRead)]
    [ApiCosts(0)]
    public IActionResult CollaborationDocument(string app, DomainId commentsId)
    {
        return new YDotNetActionResult(notificationPublisher.ResourceDocument(App.NamedId(), commentsId));
    }
}
