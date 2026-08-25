// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class WorkflowDefinition
{
    public Workflow Workflow { get; }

    public IReadOnlyDictionary<Status, StatusInfo> StatusInfos { get; }

    public StatusInfo[] AllStatuses { get; }

    public bool ValidateOnPublish { get; }

    public WorkflowDefinition(Workflow workflow, bool validateOnPublish)
    {
        // The status infos never change for a workflow, therefore they are created once and shared
        // by all contents instead of allocating them for every single status lookup.
        var statusInfos = new Dictionary<Status, StatusInfo>(workflow.Steps.Count);

        foreach (var (status, step) in workflow.Steps)
        {
            statusInfos[status] = new StatusInfo(status, step.Color ?? StatusColors.Draft);
        }

        Workflow = workflow;
        StatusInfos = statusInfos;
        AllStatuses = [.. statusInfos.Values];
        ValidateOnPublish = validateOnPublish;
    }
}
