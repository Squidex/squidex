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
        Task<string[]> NormalizeTagsAsync(string[] names, string[] ids);

        Task<Dictionary<string, string>> DenormalizeTagsAsync(string[] ids);

        Task<Dictionary<string, int>> GetTagsAsync();
    }
}
