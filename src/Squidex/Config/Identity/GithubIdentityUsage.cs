// ==========================================================================
//  GithubIdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Squidex.Config.Identity
{
    public static class GitHubIdentityUsage
    {
        public static IApplicationBuilder UseMyGithubAuthentication(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            if (options.IsGithubAuthConfigured())
            {
                var githubOptions =
                    new GitHubAuthenticationOptions
                    {
                        ClientId = options.GithubClient,
                        ClientSecret = options.GithubSecret
                    };

                app.UseGitHubAuthentication(githubOptions);
            }

            return app;
        }
    }
}
