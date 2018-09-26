// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Extensions.Actions.Twitter;
using static CoreTweet.OAuth;

namespace Squidex.Areas.Api.Controllers.Rules
{
    public sealed class TwitterController : Controller
    {
        private readonly TwitterOptions twitterOptions;

        public TwitterController(IOptions<TwitterOptions> twitterOptions)
        {
            this.twitterOptions = twitterOptions.Value;
        }

        public sealed class TokenRequest
        {
            public string PinCode { get; set; }

            public string RequestToken { get; set; }

            public string RequestTokenSecret { get; set; }
        }

        [HttpGet]
        [Route("rules/twitter/auth")]
        public async Task<IActionResult> Auth()
        {
            var session = await AuthorizeAsync(twitterOptions.ClientId, twitterOptions.ClientSecret);

            return Ok(new
            {
                session.AuthorizeUri,
                session.RequestToken,
                session.RequestTokenSecret
            });
        }

        [HttpPost]
        [Route("rules/twitter/token")]
        public async Task<IActionResult> AuthComplete([FromBody] TokenRequest request)
        {
            var session = new OAuthSession
            {
                ConsumerKey = twitterOptions.ClientId,
                ConsumerSecret = twitterOptions.ClientSecret,
                RequestToken = request.RequestToken,
                RequestTokenSecret = request.RequestTokenSecret
            };

            var tokens = await session.GetTokensAsync(request.PinCode);

            return Ok(new
            {
                tokens.AccessToken,
                tokens.AccessTokenSecret
            });
        }
    }
}
