// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppWorkflowAdded))]
    public sealed class AppWorkflowAdded : AppEvent
    {
        public DomainId WorkflowId { get; set; }

        public string Name { get; set; }
    }
}
