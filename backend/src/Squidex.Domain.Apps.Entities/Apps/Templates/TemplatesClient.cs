// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed partial class TemplatesClient(IHttpClientFactory httpClientFactory, IOptions<TemplatesOptions> options)
{
    private static readonly Regex RegexTemplate = BuildTemplateRegex();
    private readonly TemplatesOptions options = options.Value;

    public async Task<string?> GetRepositoryUrl(string name,
        CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient();

        foreach (var repository in options.Repositories.OrEmpty())
        {
            var url = $"{repository.ContentUrl}/README.md";

            var text = await httpClient.GetStringAsync(url, ct);

            foreach (var match in RegexTemplate.Matches(text).OfType<Match>())
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

    public async Task<List<Template>> GetTemplatesAsync(bool includeDetails = false,
        CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient();

        var result = new List<Template>();

        foreach (var repository in options.Repositories.OrEmpty())
        {
            var url = $"{repository.ContentUrl}/README.md";

            var text = await httpClient.GetStringAsync(url, ct);

            foreach (var match in RegexTemplate.Matches(text).OfType<Match>())
            {
                var templateName = match.Groups["Name"].Value;
                var templateTitle = match.Groups["Title"].Value;

                const string StarterPrefix = "Starter ";
                var isStarter = templateTitle.StartsWith(StarterPrefix, StringComparison.OrdinalIgnoreCase);
                if (isStarter)
                {
                    templateTitle = templateTitle[StarterPrefix.Length..].TrimStart(' ', ':');
                }

                var (details, logo) = await GetDetailCoreAsync(templateName, ct);

                result.Add(new Template(
                    templateName,
                    templateTitle,
                    match.Groups["Description"].Value,
                    GetSummary(details, includeDetails),
                    isStarter,
                    logo));
            }
        }

        return result;
    }

    public async Task<string?> GetDetailAsync(string name,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(name);

        var (text, _) = await GetDetailCoreAsync(name, ct);
        return text;
    }

    private async Task<(string? Text, string? Logo)> GetDetailCoreAsync(string name,
        CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient();

        foreach (var repository in options.Repositories.OrEmpty())
        {
            var url = new Uri($"{repository.ContentUrl}/{name}/README.md", UriKind.Absolute);

            var response = await httpClient.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync(ct);

                string? logo = null;
                text = BuildLogoRegex().Replace(text, match =>
                {
                    var imageRelative = new Uri(match.Groups["Url"].Value, UriKind.Relative);
                    var imageAbsolute = new Uri(url, imageRelative);

                    logo = imageAbsolute.ToString();
                    return string.Empty;
                });

                return (text, logo);
            }
        }

        return default;
    }

    private static string GetSummary(string? markdown, bool includeDetails)
    {
        if (string.IsNullOrWhiteSpace(markdown) || !includeDetails)
        {
            return string.Empty;
        }

        var document = Markdown.Parse(markdown);
        var outputWriter = new StringWriter();
        var outputRenderer = new NormalizeRenderer(outputWriter);
        var headerCount = 0;
        foreach (var block in document)
        {
            if (block is HeadingBlock heading || IsUsageBlock(block))
            {
                headerCount++;
                if (headerCount == 2)
                {
                    break;
                }
            }
            else if (headerCount == 1)
            {
                outputRenderer.Render(block);
            }
        }

        static bool IsUsageBlock(Block block)
        {
            return block is ParagraphBlock p && IsUsageLiteral(p.Inline);
        }

        static bool IsUsageLiteral(Inline? inline)
        {
            if (inline is LiteralInline literal)
            {
                return literal.Content.AsSpan().Trim().Equals("Usage", StringComparison.Ordinal);
            }

            if (inline is ContainerInline container)
            {
                foreach (var child in container)
                {
                    if (IsUsageLiteral(child))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return outputWriter.ToString();
    }

    [GeneratedRegex("\\* \\[(?<Title>.*)\\]\\((?<Name>.*)\\/README\\.md\\): (?<Description>.*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex BuildTemplateRegex();

    [GeneratedRegex("Logo: \\[Logo\\]\\((?<Url>(.*))\\)", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex BuildLogoRegex();
}
