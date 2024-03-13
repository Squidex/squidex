// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class EnrichWithWorkflows : IContentEnricherStep
{
    private const string DefaultColor = StatusColors.Draft;

    private readonly IContentWorkflow contentWorkflow;

    public EnrichWithWorkflows(IContentWorkflow contentWorkflow)
    {
        this.contentWorkflow = contentWorkflow;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        var cache = new Dictionary<(DomainId, Status), StatusInfo>();

        foreach (var content in contents)
        {
            ct.ThrowIfCancellationRequested();

            await EnrichColorAsync(content, cache);

            if (ShouldEnrichWithStatuses(context))
            {
                await EnrichNextsAsync(content, context);
                await EnrichCanUpdateAsync(content, context);
            }
        }
    }

    private async Task EnrichNextsAsync(EnrichedContent content, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        if (content.IsSingleton)
        {
            if (editingStatus == Status.Draft)
            {
                content.NextStatuses =
                [
                    new StatusInfo(Status.Published, StatusColors.Published)
                ];
            }
            else
            {
                content.NextStatuses = [];
            }
        }
        else
        {
            content.NextStatuses = await contentWorkflow.GetNextAsync(content, editingStatus, context.UserPrincipal);
        }
    }

    private async Task EnrichCanUpdateAsync(EnrichedContent content, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        content.CanUpdate = await contentWorkflow.CanUpdateAsync(content, editingStatus, context.UserPrincipal);
    }

    private async Task EnrichColorAsync(EnrichedContent content, Dictionary<(DomainId, Status), StatusInfo> cache)
    {
        content.StatusColor = await GetColorAsync(content, content.Status, cache);

        if (content.NewStatus != null)
        {
            content.NewStatusColor = await GetColorAsync(content, content.NewStatus.Value, cache);
        }

        if (content.ScheduleJob != null)
        {
            content.ScheduledStatusColor = await GetColorAsync(content, content.ScheduleJob.Status, cache);
        }
    }

    private async Task<string> GetColorAsync(Content content, Status status, Dictionary<(DomainId, Status), StatusInfo> cache)
    {
        if (!cache.TryGetValue((content.SchemaId.Id, status), out var info))
        {
            info = await contentWorkflow.GetInfoAsync(content, status);

            if (info == null)
            {
                info = new StatusInfo(status, DefaultColor);
            }

            cache[(content.SchemaId.Id, status)] = info;
        }

        return info.Color;
    }

    private static bool ShouldEnrichWithStatuses(Context context)
    {
        return context.IsFrontendClient || context.ResolveFlow();
    }
}
