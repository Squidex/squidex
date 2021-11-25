// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Domain.Apps.Core.Tags;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public interface ITagGrain : IGrainWithStringKey
    {
        Task<Dictionary<string, string>> NormalizeTagsAsync(HashSet<string>? names, HashSet<string>? ids);

        Task<Dictionary<string, string>> GetTagIdsAsync(HashSet<string> names);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(HashSet<string> ids);

        Task<TagsSet> GetTagsAsync();

        Task<TagsExport> GetExportableTagsAsync();

        Task ClearAsync();

        Task RenameTagAsync(string name, string newName);

        Task RebuildAsync(TagsExport export);
    }
}
