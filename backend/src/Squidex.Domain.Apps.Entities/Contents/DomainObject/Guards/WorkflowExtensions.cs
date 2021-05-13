// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class WorkflowExtensions
    {
        public static Task<Status> GetInitialStatusAsync(this OperationContext context)
        {
            var workflow = GetWorkflow(context);

            return workflow.GetInitialStatusAsync(context.Schema);
        }

        public static async Task CheckTransitionAsync(this OperationContext context, Status status)
        {
            if (context.SchemaDef.Type != SchemaType.Singleton)
            {
                var workflow = GetWorkflow(context);

                var oldStatus = context.Content.EditingStatus();

                if (!await workflow.CanMoveToAsync(context.Content, oldStatus, status, context.User))
                {
                    var values = new { oldStatus, newStatus = status };

                    context.AddError(T.Get("contents.statusTransitionNotAllowed", values), nameof(status));
                    context.ThrowOnErrors();
                }
            }
        }

        public static async Task CheckStatusAsync(this OperationContext context, Status status)
        {
            if (context.SchemaDef.Type != SchemaType.Singleton)
            {
                var workflow = GetWorkflow(context);

                var statusInfo = await workflow.GetInfoAsync(context.Content, status);

                if (statusInfo == null)
                {
                    context.AddError(T.Get("contents.statusNotValid"), nameof(status));
                    context.ThrowOnErrors();
                }
            }
        }

        public static async Task CheckUpdateAsync(this OperationContext context)
        {
            if (context.User != null)
            {
                var workflow = GetWorkflow(context);

                var status = context.Content.EditingStatus();

                if (!await workflow.CanUpdateAsync(context.Content, status, context.User))
                {
                    throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status }));
                }
            }
        }

        private static IContentWorkflow GetWorkflow(OperationContext context)
        {
            return context.Resolve<IContentWorkflow>();
        }
    }
}
