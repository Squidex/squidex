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

public static class ContentHeaders
{
    private static readonly char[] Separators = { ',', ';' };

    public const string Fields = "X-Fields";
    public const string Flatten = "X-Flatten";
    public const string Languages = "Languages";
    public const string NoCleanup = "X-NoCleanup";
    public const string NoEnrichment = "X-NoEnrichment";
    public const string NoResolveLanguages = "X-NoResolveLanguages";
    public const string ResolveFlow = "X-ResolveFlow";
    public const string ResolveUrls = "X-ResolveUrls";
    public const string Unpublished = "X-Unpublished";

    public static void AddCacheHeaders(this Context context, IRequestCache cache)
    {
        cache.AddHeader(Fields);
        cache.AddHeader(Flatten);
        cache.AddHeader(Languages);
        cache.AddHeader(NoCleanup);
        cache.AddHeader(NoEnrichment);
        cache.AddHeader(NoResolveLanguages);
        cache.AddHeader(ResolveFlow);
        cache.AddHeader(ResolveUrls);
        cache.AddHeader(Unpublished);
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
        return context.Headers.ContainsKey(NoCleanup);
    }

    public static ICloneBuilder WithoutCleanup(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoCleanup, value);
    }

    public static bool ShouldSkipContentEnrichment(this Context context)
    {
        return context.Headers.ContainsKey(NoEnrichment);
    }

    public static ICloneBuilder WithoutContentEnrichment(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoEnrichment, value);
    }

    public static bool ShouldProvideUnpublished(this Context context)
    {
        return context.Headers.ContainsKey(Unpublished);
    }

    public static ICloneBuilder WithUnpublished(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(Unpublished, value);
    }

    public static bool ShouldFlatten(this Context context)
    {
        return context.Headers.ContainsKey(Flatten);
    }

    public static ICloneBuilder WithFlatten(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(Flatten, value);
    }

    public static bool ShouldResolveFlow(this Context context)
    {
        return context.Headers.ContainsKey(ResolveFlow);
    }

    public static ICloneBuilder WithResolveFlow(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(ResolveFlow, value);
    }

    public static bool ShouldResolveLanguages(this Context context)
    {
        return !context.Headers.ContainsKey(NoResolveLanguages);
    }

    public static ICloneBuilder WithoutResolveLanguages(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoResolveLanguages, value);
    }

    public static IEnumerable<string> AssetUrls(this Context context)
    {
        if (context.Headers.TryGetValue(ResolveUrls, out var value))
        {
            return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet();
        }

        return Enumerable.Empty<string>();
    }

    public static ICloneBuilder WithAssetUrlsToResolve(this ICloneBuilder builder, IEnumerable<string>? fieldNames)
    {
        return builder.WithStrings(ResolveUrls, fieldNames);
    }

    public static HashSet<string>? FieldsList(this Context context)
    {
        if (context.Headers.TryGetValue(Fields, out var value))
        {
            return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        }

        return null;
    }

    public static ICloneBuilder WithFields(this ICloneBuilder builder, IEnumerable<string> fields)
    {
        return builder.WithStrings(Fields, fields);
    }

    public static HashSet<Language> LanguagesList(this Context context)
    {
        if (context.Headers.TryGetValue(Languages, out var value))
        {
            return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => (Language)x).ToHashSet();
        }

        return new HashSet<Language>();
    }

    public static ICloneBuilder WithLanguages(this ICloneBuilder builder, IEnumerable<string> languages)
    {
        return builder.WithStrings(Languages, languages);
    }
}
