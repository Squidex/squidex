// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using CoreTweet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Rules.Actions;

namespace Squidex.Areas.Api.Controllers.Rules
{
    public sealed class TwitterController : Controller
    {
        private readonly TwitterOptions twitterOptions;

        public TwitterController(IOptions<TwitterOptions> twitterOptions)
        {
            this.twitterOptions = twitterOptions.Value;
        }

        [Route("rules/twitter/auth")]
        public async Task<IActionResult> Auth()
        {
            var session = await OAuth.AuthorizeAsync(twitterOptions.ClientId, twitterOptions.ClientSecret);

            return Redirect(session.AuthorizeUri.ToString());
        }
    }
}
