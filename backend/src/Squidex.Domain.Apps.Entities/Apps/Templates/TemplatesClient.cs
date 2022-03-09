// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Text.RegularExpressions;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class TemplatesClient
    {
        private const string DetailUrl = "https://raw.githubusercontent.com/Squidex/templates/main";
        private const string OverviewUrl = "https://raw.githubusercontent.com/Squidex/templates/main/README.md";
        private static readonly Regex Regex = new Regex("\\* \\[(?<Title>.*)\\]\\((?<Name>.*)\\/README\\.md\\): (?<Description>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly IHttpClientFactory httpClientFactory;

        public TemplatesClient(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<List<Template>> GetTemplatesAsync(
            CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var url = OverviewUrl;

                var text = await httpClient.GetStringAsync(url, ct);

                var result = new List<Template>();

                foreach (Match match in Regex.Matches(text))
                {
                    result.Add(new Template(
                        match.Groups["Name"].Value,
                        match.Groups["Title"].Value,
                        match.Groups["Description"].Value));
                }

                return result;
            }
        }

        public async Task<string?> GetDetailAsync(string name,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name);

            using (var httpClient = httpClientFactory.CreateClient())
            {
                var url = $"{DetailUrl}/{name}/README.md";

                var response = await httpClient.GetAsync(url, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync(ct);
            }
        }
    }
}
