// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class WorkflowExtensions
    {
        public static Task<Status> GetInitialStatusAsync(this OperationContext context)
        {
            var contentWorkflow = context.Services.GetRequiredService<IContentWorkflow>();

            return contentWorkflow.GetInitialStatusAsync(context.Schema);
        }

        public static async Task CheckTransitionAsync(this OperationContext context, Status status)
        {
            if (!context.SchemaDef.IsSingleton)
            {
                var contentWorkflow = context.Services.GetRequiredService<IContentWorkflow>();

                var oldStatus = context.Content.EditingStatus;

                if (!await contentWorkflow.CanMoveToAsync(context.Content, oldStatus, status, context.User))
                {
                    var values = new { oldStatus, newStatus = status };

                    context.AddError(T.Get("contents.statusTransitionNotAllowed", values), nameof(status));
                    context.ThrowOnErrors();
                }
            }
        }

        public static async Task CheckStatusAsync(this OperationContext context, Status status)
        {
            if (!context.SchemaDef.IsSingleton)
            {
                var contentWorkflow = context.Services.GetRequiredService<IContentWorkflow>();

                var statusInfo = await contentWorkflow.GetInfoAsync(context.Content, status);

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
                var contentWorkflow = context.Services.GetRequiredService<IContentWorkflow>();

                var status = context.Content.EditingStatus;

                if (!await contentWorkflow.CanUpdateAsync(context.Content, status, context.User))
                {
                    throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status }));
                }
            }
        }
    }
}
