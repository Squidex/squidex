// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IEnrichedContentEntity : IContentEntity
{
    bool CanUpdate { get; }

    bool IsSingleton { get; }

    string StatusColor { get; }

    string? NewStatusColor { get; }

    string? ScheduledStatusColor { get; }

    string SchemaDisplayName { get; }

    string? EditToken { get; }

    RootField[]? ReferenceFields { get; }

    StatusInfo[]? NextStatuses { get; }

    ContentData? ReferenceData { get; }
}
