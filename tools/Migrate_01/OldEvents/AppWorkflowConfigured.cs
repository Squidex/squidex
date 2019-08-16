// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AppWorkflowConfigured))]
    [Obsolete]
    public sealed class AppWorkflowConfigured : AppEvent, IMigrated<IEvent>
    {
        public Workflow Workflow { get; set; }

        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new AppWorkflowUpdated());
        }
    }
}
