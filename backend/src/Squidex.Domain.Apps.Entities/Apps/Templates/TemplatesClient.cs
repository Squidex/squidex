// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class TemplatesClient
{
    private static readonly Regex Regex = new Regex("\\* \\[(?<Title>.*)\\]\\((?<Name>.*)\\/README\\.md\\): (?<Description>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private readonly IHttpClientFactory httpClientFactory;
    private readonly TemplatesOptions options;

    public TemplatesClient(IHttpClientFactory httpClientFactory, IOptions<TemplatesOptions> options)
    {
        this.httpClientFactory = httpClientFactory;

        this.options = options.Value;
    }

    public async Task<string?> GetRepositoryUrl(string name,
        CancellationToken ct = default)
    {
        using (var httpClient = httpClientFactory.CreateClient())
        {
            var result = new List<Template>();

            foreach (var repository in options.Repositories.OrEmpty())
            {
                var url = $"{repository.ContentUrl}/README.md";

                var text = await httpClient.GetStringAsync(url, ct);

                foreach (Match match in Regex.Matches(text).OfType<Match>())
                {
                    var currentName = match.Groups["Name"].Value;

                    if (currentName == name)
                    {
                        return $"{repository.GitUrl ?? repository.ContentUrl}?folder={name}";
                    }
                }
            }

            return null;
        }
    }

    public async Task<List<Template>> GetTemplatesAsync(
        CancellationToken ct = default)
    {
        using (var httpClient = httpClientFactory.CreateClient())
        {
            var result = new List<Template>();

            foreach (var repository in options.Repositories.OrEmpty())
            {
                var url = $"{repository.ContentUrl}/README.md";

                var text = await httpClient.GetStringAsync(url, ct);

                foreach (Match match in Regex.Matches(text).OfType<Match>())
                {
                    var title = match.Groups["Title"].Value;

                    result.Add(new Template(
                        match.Groups["Name"].Value,
                        title,
                        match.Groups["Description"].Value,
                        title.StartsWith("Starter ", StringComparison.OrdinalIgnoreCase)));
                }
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
            foreach (var repository in options.Repositories.OrEmpty())
            {
                var url = $"{repository.ContentUrl}/{name}/README.md";

                var response = await httpClient.GetAsync(url, ct);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(ct);
                }
            }
        }

        return null;
    }
}
