// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdateWorkflow : AppCommand
    {
        public Guid WorkflowId { get; set; }

        public Workflow Workflow { get; set; }
    }
}
