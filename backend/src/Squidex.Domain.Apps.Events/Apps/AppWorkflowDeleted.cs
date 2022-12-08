// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppWorkflowDeleted))]
public sealed class AppWorkflowDeleted : AppEvent
{
    public DomainId WorkflowId { get; set; }
}
