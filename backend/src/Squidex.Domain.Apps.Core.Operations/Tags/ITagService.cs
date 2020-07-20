// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Tags
{
    public interface ITagService
    {
        Task<Dictionary<string, string>> GetTagIdsAsync(DomainId appId, string group, HashSet<string> names);

        Task<Dictionary<string, string>> NormalizeTagsAsync(DomainId appId, string group, HashSet<string>? names, HashSet<string>? ids);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(DomainId appId, string group, HashSet<string> ids);

        Task<TagsSet> GetTagsAsync(DomainId appId, string group);

        Task<TagsExport> GetExportableTagsAsync(DomainId appId, string group);

        Task RebuildTagsAsync(DomainId appId, string group, TagsExport tags);

        Task ClearAsync(DomainId appId, string group);
    }
}
