// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public static class Helper
    {
        public static string RelativeName(FileInfo file, DirectoryInfo folder)
        {
            return file.FullName[folder.FullName.Length..].Replace("\\", "/");
        }

        public static void CheckOtherLocales(TranslationService service)
        {
            var mainTranslations = service.MainTranslations;

            foreach (var (locale, texts) in service.Translations.Where(x => x.Key != service.MainLocale))
            {
                Console.WriteLine();
                Console.WriteLine("----- CHECKING <{0}> -----", locale);

                var notTranslated = mainTranslations.Keys.Except(texts.Keys).ToList();
                var notUsed = texts.Keys.Except(mainTranslations.Keys).ToList();

                if (notTranslated.Count > 0 || notUsed.Count > 0)
                {
                    if (notTranslated.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Translations missing:");

                        foreach (var key in notTranslated.OrderBy(x => x))
                        {
                            Console.Write(" * ");
                            Console.WriteLine(key);
                        }
                    }

                    if (notUsed.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Translations not used:");

                        foreach (var key in notUsed.OrderBy(x => x))
                        {
                            Console.Write(" * ");
                            Console.WriteLine(key);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("> No errors found");
                }
            }
        }

        public static void CleanOtherLocales(TranslationService service)
        {
            var mainTranslations = service.MainTranslations;

            foreach (var (locale, texts) in service.Translations.Where(x => x.Key != service.MainLocale))
            {
                Console.WriteLine();
                Console.WriteLine("----- CLEANING <{0}> -----", locale);

                var notUsed = texts.Keys.Except(mainTranslations.Keys).ToList();

                if (notUsed.Count > 0)
                {
                    foreach (var unused in notUsed)
                    {
                        texts.Remove(unused);
                    }

                    Console.WriteLine("Cleaned {0} translations.", notUsed.Count);
                }
                else
                {
                    Console.WriteLine("> No errors found");
                }
            }
        }

        public static void CheckUnused(TranslationService service, HashSet<string> translations)
        {
            var notUsed = new SortedSet<string>();

            foreach (var key in service.MainTranslations.Keys)
            {
                if (!translations.Contains(key) &&
                    !key.StartsWith("common.", StringComparison.OrdinalIgnoreCase) &&
                    !key.StartsWith("dotnet_", StringComparison.OrdinalIgnoreCase) &&
                    !key.StartsWith("validation.", StringComparison.OrdinalIgnoreCase))
                {
                    notUsed.Add(key);
                }
            }

            if (notUsed.Count > 0)
            {
                Console.WriteLine("Translations not used:");

                foreach (var key in notUsed)
                {
                    Console.Write(" * ");
                    Console.WriteLine(key);
                }
            }
        }

        public static void CheckForFile(TranslationService service, string relativeName, HashSet<string> translations)
        {
            if (translations.Count > 0)
            {
                var prefixes = new HashSet<string>();

                foreach (var key in translations.ToList())
                {
                    if (service.MainTranslations.ContainsKey(key))
                    {
                        translations.Remove(key);
                    }

                    var parts = key.Split(".");

                    if (parts.Length > 1 && parts[0] != "common" && parts[0] != "validation")
                    {
                        prefixes.Add(parts[0]);
                    }
                }

                if (HasInvalidPrefixes(prefixes) || translations.Count > 0)
                {
                    Console.WriteLine("Errors in file {0}.", relativeName);

                    if (HasInvalidPrefixes(prefixes))
                    {
                        Console.WriteLine(" > Multiple prefixes found: {0}", string.Join(",", prefixes));
                    }

                    if (translations.Count > 0)
                    {
                        foreach (var key in translations)
                        {
                            Console.Write(" * ");
                            Console.WriteLine(key);
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        private static bool HasInvalidPrefixes(HashSet<string> prefixes)
        {
            return prefixes.Count > 1;
        }
    }
}
