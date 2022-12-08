// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

public sealed class StringReferenceExtractor
{
    private readonly List<Regex> contentsPatterns = new List<Regex>();
    private readonly List<Regex> assetsPatterns = new List<Regex>();

    public StringReferenceExtractor(IUrlGenerator urlGenerator)
    {
        AddAssetPattern(@"assets?:(?<Id>[a-z0-9\-_9]+)");
        AddAssetUrlPatterns(urlGenerator.AssetContentBase());
        AddAssetUrlPatterns(urlGenerator.AssetContentCDNBase());

        AddContentPattern(@"contents?:(?<Id>[a-z0-9\-_9]+)");
        AddContentUrlPatterns(urlGenerator.ContentBase());
        AddContentUrlPatterns(urlGenerator.ContentCDNBase());
    }

    private void AddContentUrlPatterns(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        baseUrl = Regex.Escape(baseUrl);

        AddContentPattern(baseUrl + @"([^\/]+)\/([^\/]+)\/(?<Id>[a-z0-9\-_9]+)");
    }

    private void AddAssetUrlPatterns(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        baseUrl = Regex.Escape(baseUrl);

        AddAssetPattern(baseUrl + @"(?<Id>[a-z0-9\-_9]+)");
        AddAssetPattern(baseUrl + @"([^\/]+)\/(?<Id>[a-z0-9\-_9]+)");
    }

    private void AddAssetPattern(string pattern)
    {
        assetsPatterns.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture));
    }

    private void AddContentPattern(string pattern)
    {
        contentsPatterns.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture));
    }

    public IEnumerable<DomainId> GetEmbeddedContentIds(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var pattern in contentsPatterns)
        {
            foreach (Match match in pattern.Matches(text).OfType<Match>())
            {
                yield return DomainId.Create(match.Groups["Id"].Value);
            }
        }
    }

    public IEnumerable<DomainId> GetEmbeddedAssetIds(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var pattern in assetsPatterns)
        {
            foreach (Match match in pattern.Matches(text).OfType<Match>())
            {
                yield return DomainId.Create(match.Groups["Id"].Value);
            }
        }
    }
}
