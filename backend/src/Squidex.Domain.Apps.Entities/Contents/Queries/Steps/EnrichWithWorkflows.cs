// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class EnrichWithWorkflows(IContentWorkflows contentWorkflows) : IContentEnricherStep
{
    private const string DefaultColor = StatusColors.Draft;

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        var withStatuses = ShouldEnrichWithStatuses(context);

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            ct.ThrowIfCancellationRequested();

            var (schema, _) = await schemas(group.Key);

            // The workflow is resolved once per schema and caches the compiled conditions and the
            // status colors for all contents of the group.
            using var workflow = await contentWorkflows.GetWorkflowAsync(context.App, schema, ct);

            foreach (var content in group)
            {
                ct.ThrowIfCancellationRequested();

                EnrichColor(content, workflow);

                if (withStatuses)
                {
                    EnrichNexts(content, workflow, context);
                    EnrichCanUpdate(content, workflow, context);
                }
            }
        }
    }

    private static void EnrichNexts(EnrichedContent content, IContentWorkflow workflow, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        if (content.IsSingleton)
        {
            if (editingStatus == Status.Draft)
            {
                content.NextStatuses =
                [
                    new StatusInfo(Status.Published, StatusColors.Published),
                ];
            }
            else
            {
                content.NextStatuses = [];
            }
        }
        else
        {
            content.NextStatuses = workflow.GetNext(content, editingStatus, context.UserPrincipal);
        }
    }

    private static void EnrichCanUpdate(EnrichedContent content, IContentWorkflow workflow, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        content.CanUpdate = workflow.CanUpdate(content, editingStatus, context.UserPrincipal);
    }

    private static void EnrichColor(EnrichedContent content, IContentWorkflow workflow)
    {
        content.StatusColor = GetColor(workflow, content.Status);

        if (content.NewStatus != null)
        {
            content.NewStatusColor = GetColor(workflow, content.NewStatus.Value);
        }

        if (content.ScheduleJob != null)
        {
            content.ScheduledStatusColor = GetColor(workflow, content.ScheduleJob.Status);
        }
    }

    private static string GetColor(IContentWorkflow workflow, Status status)
    {
        return workflow.GetInfo(status)?.Color ?? DefaultColor;
    }

    private static bool ShouldEnrichWithStatuses(Context context)
    {
        return context.IsFrontendClient || context.ResolveFlow();
    }
}
