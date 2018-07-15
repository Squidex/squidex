// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public interface ITagService
    {
        Task<HashSet<string>> NormalizeTagsAsync(Guid appId, string category, HashSet<string> names, HashSet<string> ids);

        Task<HashSet<string>> GetTagIdsAsync(Guid appId, string category, HashSet<string> names);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(Guid appId, string category, HashSet<string> ids);

        Task<Dictionary<string, int>> GetTagsAsync(Guid appId, string category);
    }
}
