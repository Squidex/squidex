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
            return file.FullName.Substring(folder.FullName.Length).Replace("\\", "/");
        }

        public static void CheckUnused(TranslationService service, HashSet<string> translations)
        {
            var notUsed = new SortedSet<string>();

            foreach (var key in service.MainTranslations.Keys)
            {
                if (!translations.Contains(key) && !key.StartsWith("validation.", StringComparison.OrdinalIgnoreCase))
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

                    if (parts[0] != "common" && parts[0] != "validation")
                    {
                        prefixes.Add(parts[0]);
                    }
                }

                if (HasInvalidPrefixes(prefixes) || translations.Count > 0)
                {
                    Console.WriteLine("Errors in {0}.", relativeName);

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
