﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateWorkflowDto
    {
        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The workflow steps.
        /// </summary>
        [LocalizedRequired]
        public Dictionary<Status, WorkflowStepDto> Steps { get; set; }

        /// <summary>
        /// The schema ids.
        /// </summary>
        public List<Guid>? SchemaIds { get; set; }

        /// <summary>
        /// The initial step.
        /// </summary>
        [LocalizedRequired]
        public Status Initial { get; set; }

        public UpdateWorkflow ToCommand(Guid id)
        {
            var workflow = new Workflow(
                Initial,
                Steps?.ToDictionary(
                    x => x.Key,
                    x => x.Value?.ToStep()!),
                SchemaIds,
                Name);

            return new UpdateWorkflow { WorkflowId = id, Workflow = workflow };
        }
    }
}
