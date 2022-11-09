// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Contents;

public static class ContentExtensions
{
    private const string HeaderFlatten = "X-Flatten";
    private const string HeaderLanguages = "X-Languages";
    private const string HeaderNoCleanup = "X-NoCleanup";
    private const string HeaderNoEnrichment = "X-NoEnrichment";
    private const string HeaderNoResolveLanguages = "X-NoResolveLanguages";
    private const string HeaderResolveFlow = "X-ResolveFlow";
    private const string HeaderResolveUrls = "X-Resolve-Urls";
    private const string HeaderUnpublished = "X-Unpublished";
    private static readonly char[] Separators = { ',', ';' };

    public static void AddCacheHeaders(this Context context, IRequestCache cache)
    {
        cache.AddHeader(HeaderFlatten);
        cache.AddHeader(HeaderLanguages);
        cache.AddHeader(HeaderNoCleanup);
        cache.AddHeader(HeaderNoEnrichment);
        cache.AddHeader(HeaderNoResolveLanguages);
        cache.AddHeader(HeaderResolveFlow);
        cache.AddHeader(HeaderResolveUrls);
        cache.AddHeader(HeaderUnpublished);
    }

    public static Status EditingStatus(this IContentEntity content)
    {
        return content.NewStatus ?? content.Status;
    }

    public static bool IsPublished(this IContentEntity content)
    {
        return content.EditingStatus() == Status.Published;
    }

    public static SearchScope Scope(this Context context)
    {
        return context.ShouldProvideUnpublished() || context.IsFrontendClient ? SearchScope.All : SearchScope.Published;
    }

    public static bool ShouldSkipCleanup(this Context context)
    {
        return context.Headers.ContainsKey(HeaderNoCleanup);
    }

    public static ICloneBuilder WithoutCleanup(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderNoCleanup, value);
    }

    public static bool ShouldSkipContentEnrichment(this Context context)
    {
        return context.Headers.ContainsKey(HeaderNoEnrichment);
    }

    public static ICloneBuilder WithoutContentEnrichment(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderNoEnrichment, value);
    }

    public static bool ShouldProvideUnpublished(this Context context)
    {
        return context.Headers.ContainsKey(HeaderUnpublished);
    }

    public static ICloneBuilder WithUnpublished(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderUnpublished, value);
    }

    public static bool ShouldFlatten(this Context context)
    {
        return context.Headers.ContainsKey(HeaderFlatten);
    }

    public static ICloneBuilder WithFlatten(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderFlatten, value);
    }

    public static bool ShouldResolveFlow(this Context context)
    {
        return context.Headers.ContainsKey(HeaderResolveFlow);
    }

    public static ICloneBuilder WithResolveFlow(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderResolveFlow, value);
    }

    public static bool ShouldResolveLanguages(this Context context)
    {
        return !context.Headers.ContainsKey(HeaderNoResolveLanguages);
    }

    public static ICloneBuilder WithoutResolveLanguages(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderNoResolveLanguages, value);
    }

    public static IEnumerable<string> AssetUrls(this Context context)
    {
        if (context.Headers.TryGetValue(HeaderResolveUrls, out var value))
        {
            return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet();
        }

        return Enumerable.Empty<string>();
    }

    public static ICloneBuilder WithAssetUrlsToResolve(this ICloneBuilder builder, IEnumerable<string>? fieldNames)
    {
        return builder.WithStrings(HeaderResolveUrls, fieldNames);
    }

    public static IEnumerable<Language> Languages(this Context context)
    {
        if (context.Headers.TryGetValue(HeaderLanguages, out var value))
        {
            var languages = new HashSet<Language>();

            foreach (var iso2Code in value.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            {
                languages.Add(iso2Code);
            }

            return languages;
        }

        return Enumerable.Empty<Language>();
    }

    public static ICloneBuilder WithLanguages(this ICloneBuilder builder, IEnumerable<string> fieldNames)
    {
        return builder.WithStrings(HeaderLanguages, fieldNames);
    }
}
