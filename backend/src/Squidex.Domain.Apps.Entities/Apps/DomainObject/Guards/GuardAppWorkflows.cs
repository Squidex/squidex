// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
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

        public static void CanUpdate(UpdateWorkflow command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var workflows = app.Workflows;

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
                    e(T.Get("workflows.publishedIsInitial"), $"{nameof(command.Workflow)}.{nameof(workflow.Initial)}");
                }

                var stepsPrefix = $"{nameof(command.Workflow)}.{nameof(workflow.Steps)}";

                if (!workflow.Steps.ContainsKey(Status.Published))
                {
                    e(T.Get("workflows.publishedNotDefined"), stepsPrefix);
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
                                e(T.Get("workflows.publishedStepNotFound"), transitionPrefix);
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

        public static void CanDelete(DeleteWorkflow command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var workflows = app.Workflows;

            CheckWorkflowExists(workflows, command.WorkflowId);
        }

        private static void CheckWorkflowExists(Workflows workflows, DomainId id)
        {
            if (!workflows.ContainsKey(id))
            {
                throw new DomainObjectNotFoundException(id.ToString());
            }
        }
    }
}
