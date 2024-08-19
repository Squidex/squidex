// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Contents;

public static class ContentHeaders
{
    public const string KeyFields = "X-Fields";
    public const string KeyFlatten = "X-Flatten";
    public const string KeyLanguages = "X-Languages";
    public const string KeyNoCleanup = "X-NoCleanup";
    public const string KeyNoEnrichment = "X-NoEnrichment";
    public const string KeyNoDefaults = "X-NoDefaults";
    public const string KeyNoResolveLanguages = "X-NoResolveLanguages";
    public const string KeyResolveFlow = "X-ResolveFlow";
    public const string KeyResolveUrls = "X-ResolveUrls";
    public const string KeyResolveSchemaNames = "X-ResolveSchemaName";
    public const string KeyUnpublished = "X-Unpublished";

    public static void AddCacheHeaders(this Context context, IRequestCache cache)
    {
        cache.AddHeader(KeyFields);
        cache.AddHeader(KeyFlatten);
        cache.AddHeader(KeyLanguages);
        cache.AddHeader(KeyNoCleanup);
        cache.AddHeader(KeyNoEnrichment);
        cache.AddHeader(KeyNoDefaults);
        cache.AddHeader(KeyNoResolveLanguages);
        cache.AddHeader(KeyResolveFlow);
        cache.AddHeader(KeyResolveUrls);
        cache.AddHeader(KeyUnpublished);
    }

    public static SearchScope Scope(this Context context)
    {
        return context.Unpublished() || context.IsFrontendClient ? SearchScope.All : SearchScope.Published;
    }

    public static bool NoCleanup(this Context context)
    {
        return context.AsBoolean(KeyNoCleanup);
    }

    public static ICloneBuilder WithNoCleanup(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoCleanup, value);
    }

    public static bool NoEnrichment(this Context context)
    {
        return context.AsBoolean(KeyNoEnrichment);
    }

    public static ICloneBuilder WithNoEnrichment(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoEnrichment, value);
    }

    public static bool NoDefaults(this Context context)
    {
        return context.AsBoolean(KeyNoDefaults);
    }

    public static ICloneBuilder WithNoDefaults(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoDefaults, value);
    }

    public static bool Unpublished(this Context context)
    {
        return context.AsBoolean(KeyUnpublished);
    }

    public static ICloneBuilder WithUnpublished(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyUnpublished, value);
    }

    public static bool Flatten(this Context context)
    {
        return context.AsBoolean(KeyFlatten);
    }

    public static ICloneBuilder WithFlatten(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyFlatten, value);
    }

    public static bool ResolveFlow(this Context context)
    {
        return context.AsBoolean(KeyResolveFlow);
    }

    public static ICloneBuilder WithResolveFlow(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyResolveFlow, value);
    }

    public static bool ResolveSchemaNames(this Context context)
    {
        return context.AsBoolean(KeyResolveSchemaNames);
    }

    public static ICloneBuilder WithResolveSchemaNames(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyResolveSchemaNames, value);
    }

    public static bool NoResolveLanguages(this Context context)
    {
        return context.AsBoolean(KeyNoResolveLanguages);
    }

    public static ICloneBuilder WithNoResolveLanguages(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(KeyNoResolveLanguages, value);
    }

    public static IEnumerable<string> ResolveUrls(this Context context)
    {
        return context.AsStrings(KeyResolveUrls);
    }

    public static ICloneBuilder WithResolveUrls(this ICloneBuilder builder, IEnumerable<string>? fieldNames)
    {
        return builder.WithStrings(KeyResolveUrls, fieldNames);
    }

    public static HashSet<string>? Fields(this Context context)
    {
        return context.AsStrings(KeyFields).ToHashSet();
    }

    public static ICloneBuilder WithFields(this ICloneBuilder builder, IEnumerable<string>? fields)
    {
        return builder.WithStrings(KeyFields, fields);
    }

    public static HashSet<Language> Languages(this Context context)
    {
        return context.AsStrings(KeyLanguages).Select(Language.GetLanguage).ToHashSet();
    }

    public static ICloneBuilder WithLanguages(this ICloneBuilder builder, IEnumerable<string> languages)
    {
        return builder.WithStrings(KeyLanguages, languages);
    }
}
