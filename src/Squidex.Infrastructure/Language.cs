// =========================================================================
//  Language.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Squidex.Infrastructure
{
    public sealed class Language
    {
        private readonly string iso2Code;
        private readonly string englishName;
        private static readonly Dictionary<string, Language> allLanguages = new Dictionary<string, Language>();

        static Language()
        {
            var resourceAssembly = typeof(Language).GetTypeInfo().Assembly;
            var resourceStream = resourceAssembly.GetManifestResourceStream("Squidex.Infrastructure.language-codes.csv");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var languageLine = reader.ReadLine();
                    var languageIso2Code = languageLine.Substring(1, 2);
                    var languageEnglishName = languageLine.Substring(6, languageLine.Length - 7);

                    allLanguages[languageIso2Code] = new Language(languageIso2Code, languageEnglishName);
                }
            }
        }

        public static Language GetLanguage(string iso2Code)
        {
            Guard.NotNullOrEmpty(iso2Code, nameof(iso2Code));

            try
            {
                return allLanguages[iso2Code];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException($"Language {iso2Code} is not supported");
            }
        }

        public static IEnumerable<Language> AllLanguages
        {
            get { return allLanguages.Values; }
        }

        public string EnglishName
        {
            get { return englishName; }
        }

        public string Iso2Code
        {
            get { return iso2Code; }
        }

        private Language(string iso2Code, string englishName)
        {
            this.iso2Code = iso2Code;

            this.englishName = englishName;
        }
    }
}