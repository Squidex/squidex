﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;

namespace GenerateLanguages
{
    public static class Program
    {
        public static void Main()
        {
            var languageCodesFile = new FileInfo("../../../../../src/Squidex.Infrastructure/language-codes.csv");
            var languageFile = Path.Combine(languageCodesFile.DirectoryName, "Languages.cs");

            var writer = new StringWriter();
            writer.WriteLine("// ==========================================================================");
            writer.WriteLine("//  Languages.cs");
            writer.WriteLine("//  Squidex Headless CMS");
            writer.WriteLine("// ==========================================================================");
            writer.WriteLine("//  Copyright (c) Squidex UG (haftungsbeschränkt)");
            writer.WriteLine("//  All rights reserved. Licensed under the MIT license.");
            writer.WriteLine("// ==========================================================================");
            writer.WriteLine("// <autogenerated/>");
            writer.WriteLine();
            writer.WriteLine("using System;");
            writer.WriteLine("using System.CodeDom.Compiler;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine("namespace Squidex.Infrastructure");
            writer.WriteLine("{");
            writer.WriteLine("    [GeneratedCode(\"LanguagesGenerator\", \"1.0\")]");
            writer.WriteLine("    public partial record Language");
            writer.WriteLine("    {");
            writer.WriteLine("        private static readonly Dictionary<string, Language> AllLanguagesField = new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase);");
            writer.WriteLine("        private static readonly Dictionary<string, string> AllLanguagesNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);");
            writer.WriteLine();
            writer.WriteLine("        internal static Language AddLanguage(string iso2Code, string englishName)");
            writer.WriteLine("        {");
            writer.WriteLine("            AllLanguagesNames[iso2Code] = englishName;");
            writer.WriteLine();
            writer.WriteLine("            return AllLanguagesField.GetOrAdd(iso2Code, code => new Language(code));");
            writer.WriteLine("        }");
            writer.WriteLine();

            var languages = GetLanguages(languageCodesFile).ToList();

            foreach (var (iso2Code, englishName) in languages)
            {
                var fieldName = iso2Code.ToUpperInvariant();

                writer.WriteLine($"        public static readonly Language {fieldName} = AddLanguage(\"{iso2Code}\", \"{englishName}\");");
            }

            writer.WriteLine();

            foreach (var (code, englishName) in GetCultures(languages))
            {
                var fieldName = englishName.ToFieldName();

                writer.WriteLine($"        public static readonly Language {fieldName} = AddLanguage(\"{code}\", \"{englishName}\");");
            }

            writer.WriteLine("    }");
            writer.WriteLine("}");

            File.WriteAllText(languageFile, writer.ToString());
        }

        private static string ToFieldName(this string name)
        {
            var sb = new StringBuilder();

            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static IEnumerable<(string Code, string EnglishName)> GetCultures(List<(string Iso2Code, string EnglishName)> languages)
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Where(x => x.ToString().Length == 5)
                .Where(x => languages.Any(l => l.Iso2Code.Equals(x.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)))
                .Select(x => (x.ToString(), x.EnglishName));
        }

        private static IEnumerable<(string Iso2Code, string EnglishName)> GetLanguages(FileInfo file)
        {
            var uniqueCodes = new HashSet<string>(new[] { "iv" });

            var resourceStream = file.OpenRead();

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    var iso2Code = line.Substring(1, 2);

                    if (uniqueCodes.Add(iso2Code))
                    {
                        yield return (iso2Code, line[6..^1]);
                    }
                    else
                    {
                        Console.WriteLine("Languages contains duplicate {0}", iso2Code);
                    }
                }
            }
        }
    }
}
