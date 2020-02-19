// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public static class ContextExtensions
    {
        private const string HeaderUnpublished = "X-Unpublished";
        private const string HeaderFlatten = "X-Flatten";
        private const string HeaderLanguages = "X-Languages";
        private const string HeaderResolveFlow = "X-ResolveFlow";
        private const string HeaderResolveAssetUrls = "X-Resolve-Urls";
        private const string HeaderNoResolveLanguages = "X-NoResolveLanguages";
        private const string HeaderNoEnrichment = "X-NoEnrichment";
        private const string HeaderNoCleanup = "X-NoCleanup";
        private static readonly char[] Separators = { ',', ';' };

        public static bool ShouldCleanup(this Context context)
        {
            return !context.Headers.ContainsKey(HeaderNoCleanup);
        }

        public static Context WithoutCleanup(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderNoCleanup, value);
        }

        public static bool ShouldEnrichContent(this Context context)
        {
            return !context.Headers.ContainsKey(HeaderNoEnrichment);
        }

        public static Context WithoutContentEnrichment(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderNoEnrichment, value);
        }

        public static bool ShouldProvideUnpublished(this Context context)
        {
            return context.Headers.ContainsKey(HeaderUnpublished);
        }

        public static Context WithUnpublished(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderUnpublished, value);
        }

        public static bool ShouldFlatten(this Context context)
        {
            return context.Headers.ContainsKey(HeaderFlatten);
        }

        public static Context WithFlatten(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderFlatten, value);
        }

        public static bool ShouldResolveFlow(this Context context)
        {
            return context.Headers.ContainsKey(HeaderResolveFlow);
        }

        public static Context WithResolveFlow(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderResolveFlow, value);
        }

        public static bool ShouldResolveLanguages(this Context context)
        {
            return !context.Headers.ContainsKey(HeaderNoResolveLanguages);
        }

        public static Context WithoutResolveLanguages(this Context context, bool value = true)
        {
            return SetBoolean(context, HeaderNoResolveLanguages, value);
        }

        public static SearchScope Scope(this Context context)
        {
            return context.ShouldProvideUnpublished() || context.IsFrontendClient ? SearchScope.All : SearchScope.Published;
        }

        public static IEnumerable<string> AssetUrls(this Context context)
        {
            if (context.Headers.TryGetValue(HeaderResolveAssetUrls, out var value))
            {
                return value.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToHashSet();
            }

            return Enumerable.Empty<string>();
        }

        public static Context WithAssetUrlsToResolve(this Context context, IEnumerable<string> fieldNames)
        {
            if (fieldNames?.Any() == true)
            {
                context.Headers[HeaderResolveAssetUrls] = string.Join(",", fieldNames);
            }
            else
            {
                context.Headers.Remove(HeaderResolveAssetUrls);
            }

            return context;
        }

        public static IEnumerable<Language> Languages(this Context context)
        {
            if (context.Headers.TryGetValue(HeaderLanguages, out var value))
            {
                var languages = new HashSet<Language>();

                foreach (var iso2Code in value.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Language.TryGetLanguage(iso2Code.Trim(), out var language))
                    {
                        languages.Add(language);
                    }
                }

                return languages;
            }

            return Enumerable.Empty<Language>();
        }

        public static Context WithLanguages(this Context context, IEnumerable<string> fieldNames)
        {
            if (fieldNames?.Any() == true)
            {
                context.Headers[HeaderLanguages] = string.Join(",", fieldNames);
            }
            else
            {
                context.Headers.Remove(HeaderLanguages);
            }

            return context;
        }

        private static Context SetBoolean(Context context, string key, bool value)
        {
            if (value)
            {
                context.Headers[key] = "1";
            }
            else
            {
                context.Headers.Remove(key);
            }

            return context;
        }
    }
}
