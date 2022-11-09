// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public abstract class AppSubscription : ISubscription
{
    public DomainId AppId { get; set; }

    public PermissionSet Permissions { get; set; }

    public abstract ValueTask<bool> ShouldHandle(object message);
}
