// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Infrastructure;
using Squidex.Text.RichText.Model;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

public sealed class StringReferenceExtractor
{
    private readonly List<Regex> contentsPatterns = [];
    private readonly List<Regex> assetsPatterns = [];

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

    public IEnumerable<DomainId> GetEmbeddedContentIds(INode node)
    {
        var result = new List<DomainId>();

        static void Visit(INode node, List<DomainId> result, List<Regex> patterns)
        {
            if (node.Type == NodeType.Text)
            {
                IMark? mark = null;
                while ((mark = node.GetNextMark()) != null)
                {
                    if (mark.Type == MarkType.Link)
                    {
                        var href = mark.GetStringAttr("href", string.Empty);

                        result.AddRange(GetEmbeddedIds(href, patterns));
                    }
                }
            }

            node.IterateContent((result, patterns), static (node, s, _, _) => Visit(node, s.result, s.patterns));
        }

        Visit(node, result, contentsPatterns);
        return result;
    }

    public IEnumerable<DomainId> GetEmbeddedContentIds(string text)
    {
        return GetEmbeddedIds(text, contentsPatterns);
    }

    public IEnumerable<DomainId> GetEmbeddedAssetIds(INode node)
    {
        var result = new List<DomainId>();

        static void Visit(INode node, List<DomainId> result, List<Regex> patterns)
        {
            if (node.Type == NodeType.Image)
            {
                var src = node.GetStringAttr("src", string.Empty);

                result.AddRange(GetEmbeddedIds(src, patterns));
            }

            node.IterateContent((result, patterns), static (node, s, _, _) => Visit(node, s.result, s.patterns));
        }

        Visit(node, result, assetsPatterns);
        return result;
    }

    public IEnumerable<DomainId> GetEmbeddedAssetIds(string text)
    {
        return GetEmbeddedIds(text, assetsPatterns);
    }

    private static IEnumerable<DomainId> GetEmbeddedIds(string text, List<Regex> patterns)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var pattern in patterns)
        {
            foreach (Match match in pattern.Matches(text).OfType<Match>())
            {
                yield return DomainId.Create(match.Groups["Id"].Value);
            }
        }
    }
}
