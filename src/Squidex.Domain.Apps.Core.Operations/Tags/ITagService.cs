// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Tags
{
    public interface ITagService
    {
        Task<Dictionary<string, string>> GetTagIdsAsync(Guid appId, string group, HashSet<string> names);

        Task<Dictionary<string, string>> NormalizeTagsAsync(Guid appId, string group, HashSet<string> names, HashSet<string> ids);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(Guid appId, string group, HashSet<string> ids);

        Task<Dictionary<string, int>> GetTagsAsync(Guid appId, string group);

        Task<TagSet> GetExportableTagsAsync(Guid appId, string group);

        Task RebuildTagsAsync(Guid appId, string group, TagSet tags);

        Task ClearAsync(Guid appId, string group);
    }
}
