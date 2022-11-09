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

    public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        var cache = new Dictionary<(DomainId, Status), StatusInfo>();

        foreach (var content in contents)
        {
            ct.ThrowIfCancellationRequested();

            await EnrichColorAsync(content, content, cache);

            if (ShouldEnrichWithStatuses(context))
            {
                await EnrichNextsAsync(content, context);
                await EnrichCanUpdateAsync(content, context);
            }
        }
    }

    private async Task EnrichNextsAsync(ContentEntity content, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        if (content.IsSingleton)
        {
            if (editingStatus == Status.Draft)
            {
                content.NextStatuses = new[]
                {
                    new StatusInfo(Status.Published, StatusColors.Published)
                };
            }
            else
            {
                content.NextStatuses = Array.Empty<StatusInfo>();
            }
        }
        else
        {
            content.NextStatuses = await contentWorkflow.GetNextAsync(content, editingStatus, context.UserPrincipal);
        }
    }

    private async Task EnrichCanUpdateAsync(ContentEntity content, Context context)
    {
        var editingStatus = content.NewStatus ?? content.Status;

        content.CanUpdate = await contentWorkflow.CanUpdateAsync(content, editingStatus, context.UserPrincipal);
    }

    private async Task EnrichColorAsync(ContentEntity content, ContentEntity result, Dictionary<(DomainId, Status), StatusInfo> cache)
    {
        result.StatusColor = await GetColorAsync(content, content.Status, cache);

        if (content.NewStatus != null)
        {
            result.NewStatusColor = await GetColorAsync(content, content.NewStatus.Value, cache);
        }

        if (content.ScheduleJob != null)
        {
            result.ScheduledStatusColor = await GetColorAsync(content, content.ScheduleJob.Status, cache);
        }
    }

    private async Task<string> GetColorAsync(IContentEntity content, Status status, Dictionary<(DomainId, Status), StatusInfo> cache)
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
        return context.IsFrontendClient || context.ShouldResolveFlow();
    }
}
