// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Search.Models
{
    public class SearchResultDto : Resource
    {
        /// <summary>
        /// The name of the search result.
        /// </summary>
        [LocalizedRequired]
        public string Name { get; set; }

        /// <summary>
        /// The type of the search result.
        /// </summary>
        [LocalizedRequired]
        public SearchResultType Type { get; set; }

        /// <summary>
        /// An optional label.
        /// </summary>
        public string? Label { get; set; }

        public static SearchResultDto FromSearchResult(SearchResult searchResult)
        {
            var result = new SearchResultDto();

            SimpleMapper.Map(searchResult, result);

            result.CreateLinks(searchResult);

            return result;
        }

        protected virtual void CreateLinks(SearchResult searchResult)
        {
            AddGetLink("url", searchResult.Url);
        }
    }
}
