// ==========================================================================
//  ISearchEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.SearchEngines
{
    public interface ISearchEngine
    {
        Task<bool> AddContentToIndexAsync(JObject content, Guid contentId, string typeName, string indexName);

        Task<bool> UpdateContentInIndexAsync(JObject content, Guid contentId, string typeName, string indexName);

        Task<bool> DeleteContentFromIndexAsync(Guid contentId, string typeName, string indexName);
    }
}