// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Tags;

public interface ITagService
{
    Task<Dictionary<string, string>> GetTagIdsAsync(DomainId id, string group, HashSet<string> names,
        CancellationToken ct = default);

    Task<Dictionary<string, string>> GetTagNamesAsync(DomainId id, string group, HashSet<string> ids,
        CancellationToken ct = default);

    Task UpdateAsync(DomainId id, string group, Dictionary<string, int> updates,
        CancellationToken ct = default);

    Task<TagsSet> GetTagsAsync(DomainId id, string group,
        CancellationToken ct = default);

    Task<TagsExport> GetExportableTagsAsync(DomainId id, string group,
        CancellationToken ct = default);

    Task RenameTagAsync(DomainId id, string group, string name, string newName,
        CancellationToken ct = default);

    Task RebuildTagsAsync(DomainId id, string group, TagsExport export,
        CancellationToken ct = default);

    Task ClearAsync(DomainId id, string group,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);
}
