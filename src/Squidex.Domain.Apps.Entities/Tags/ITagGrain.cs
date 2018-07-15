// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public interface ITagGrain : IGrainWithStringKey
    {
        Task<HashSet<string>> NormalizeTagsAsync(HashSet<string> names, HashSet<string> ids);

        Task<HashSet<string>> GetTagIdsAsync(HashSet<string> names);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(HashSet<string> ids);

        Task<Dictionary<string, int>> GetTagsAsync();
    }
}
