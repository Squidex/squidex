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
        private static readonly char[] Separators = { ',', ';' };

        public static bool IsUnpublished(this Context context)
        {
            return context.Headers.ContainsKey(HeaderUnpublished);
        }

        public static Context WithUnpublished(this Context context, bool value = true)
        {
            if (value)
            {
                context.Headers[HeaderUnpublished] = "1";
            }
            else
            {
                context.Headers.Remove(HeaderUnpublished);
            }

            return context;
        }

        public static bool IsFlatten(this Context context)
        {
            return context.Headers.ContainsKey(HeaderFlatten);
        }

        public static Context WithFlatten(this Context context, bool value = true)
        {
            if (value)
            {
                context.Headers[HeaderFlatten] = "1";
            }
            else
            {
                context.Headers.Remove(HeaderFlatten);
            }

            return context;
        }

        public static bool IsResolveFlow(this Context context)
        {
            return context.Headers.ContainsKey(HeaderResolveFlow);
        }

        public static Context WithResolveFlow(this Context context, bool value = true)
        {
            if (value)
            {
                context.Headers[HeaderResolveFlow] = "1";
            }
            else
            {
                context.Headers.Remove(HeaderResolveFlow);
            }

            return context;
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
            if (context.Headers.TryGetValue(HeaderResolveAssetUrls, out var value))
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
    }
}
