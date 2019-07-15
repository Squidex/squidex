﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppWorkflowUpdated))]
    public sealed class AppWorkflowUpdated : AppEvent
    {
        public Guid WorkflowId { get; set; }

        public Workflow Workflow { get; set; }
    }
}
