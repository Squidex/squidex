// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppWorkflows
    {
        public static void CanAdd(AddWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }
            });
        }

        public static void CanUpdate(Workflows workflows, UpdateWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            CheckWorkflowExists(workflows, command.WorkflowId);

            Validate.It(e =>
            {
                if (command.Workflow == null)
                {
                    e(Not.Defined(nameof(command.Workflow)), nameof(command.Workflow));
                    return;
                }

                var workflow = command.Workflow;

                if (!workflow.Steps.ContainsKey(workflow.Initial))
                {
                    e(Not.Defined("InitialStep"), $"{nameof(command.Workflow)}.{nameof(workflow.Initial)}");
                }

                if (workflow.Initial == Status.Published)
                {
                    e(T.Get("workflows.initialNotPublished"), $"{nameof(command.Workflow)}.{nameof(workflow.Initial)}");
                }

                var stepsPrefix = $"{nameof(command.Workflow)}.{nameof(workflow.Steps)}";

                if (!workflow.Steps.ContainsKey(Status.Published))
                {
                    e(T.Get("apps.workflows.initialNotPublished"), stepsPrefix);
                }

                foreach (var step in workflow.Steps)
                {
                    var stepPrefix = $"{stepsPrefix}.{step.Key}";

                    if (step.Value == null)
                    {
                        e(Not.Defined("WorkflowStep"), stepPrefix);
                    }
                    else
                    {
                        foreach (var (status, transition) in step.Value.Transitions)
                        {
                            var transitionPrefix = $"{stepPrefix}.{nameof(step.Value.Transitions)}.{status}";

                            if (!workflow.Steps.ContainsKey(status))
                            {
                                e(T.Get("apps.workflows.publishedStepNotFound"), transitionPrefix);
                            }

                            if (transition == null)
                            {
                                e(Not.Defined("WorkflowTransition"), transitionPrefix);
                            }
                        }
                    }
                }
            });
        }

        public static void CanDelete(Workflows workflows, DeleteWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            CheckWorkflowExists(workflows, command.WorkflowId);
        }

        private static void CheckWorkflowExists(Workflows workflows, Guid id)
        {
            if (!workflows.ContainsKey(id))
            {
                throw new DomainObjectNotFoundException(id.ToString());
            }
        }
    }
}
