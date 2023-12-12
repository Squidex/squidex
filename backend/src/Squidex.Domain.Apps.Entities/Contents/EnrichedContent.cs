// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents;

public record EnrichedContent : Content
{
    public bool CanUpdate { get; set; }

    public bool IsSingleton { get; set; }

    public string StatusColor { get; set; }

    public string? NewStatusColor { get; set; }

    public string? ScheduledStatusColor { get; set; }

    public string SchemaDisplayName { get; set; }

    public string? EditToken { get; set; }

    public RootField[]? ReferenceFields { get; set; }

    public StatusInfo[]? NextStatuses { get; set; }

    public ContentData? ReferenceData { get; set; }
}
