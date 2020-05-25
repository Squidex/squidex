// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdateWorkflow : AppUpdateCommand
    {
        public DomainId WorkflowId { get; set; }

        public Workflow Workflow { get; set; }
    }
}
