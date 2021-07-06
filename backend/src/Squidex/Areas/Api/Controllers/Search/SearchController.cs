// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Search.Models;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Search
{
    /// <summary>
    /// Retrieves search results.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Search))]
    public class SearchController : ApiController
    {
        private readonly ISearchManager searchManager;

        public SearchController(ISearchManager searchManager, ICommandBus commandBus)
            : base(commandBus)
        {
            this.searchManager = searchManager;
        }

        /// <summary>
        /// Get search results.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="query">The search query.</param>
        /// <returns>
        /// 200 => Search results returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/search/")]
        [ProducesResponseType(typeof(SearchResultDto[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppSearch)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSearchResults(string app, [FromQuery] string? query = null)
        {
            var result = await searchManager.SearchAsync(query, Context, HttpContext.RequestAborted);

            var response = result.Select(SearchResultDto.FromSearchResult).ToArray();

            return Ok(response);
        }
    }
}
