// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public class CheckFrontend
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public CheckFrontend(DirectoryInfo folder, TranslationService service)
        {
            this.folder = Frontend.GetFolder(folder);

            this.service = service;
        }

        public void Run()
        {
            var all = new HashSet<string>();

            foreach (var (file, relativeName) in Frontend.GetTemplateFiles(folder))
            {
                var translations = GetTranslationsInTemplate(file);

                foreach (var translation in translations)
                {
                    all.Add(translation);
                }

                Helper.CheckForFile(service, relativeName, translations);
            }

            foreach (var (file, relativeName) in Frontend.GetTypescriptFiles(folder))
            {
                var translations = GetTranslationsInTypescript(file);

                foreach (var translation in translations)
                {
                    all.Add(translation);
                }

                Helper.CheckForFile(service, relativeName, translations);
            }

            Helper.CheckUnused(service, all);
            Helper.CheckOtherLocales(service);

            service.Save();
        }

        private HashSet<string> GetTranslationsInTemplate(FileInfo file)
        {
            var content = File.ReadAllText(file.FullName);

            var translations = new HashSet<string>();

            var matches0 = Regex.Matches(content, "\"i18n\\:(?<Key>[^\"]+)\"", RegexOptions.Singleline);

            foreach (Match match in matches0)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            var matches1 = Regex.Matches(content, "'i18n\\:(?<Key>[^\']+)'", RegexOptions.Singleline);

            foreach (Match match in matches1)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            var matches2 = Regex.Matches(content, "'(?<Key>[^\']+)' \\| sqxTranslate", RegexOptions.Singleline);

            foreach (Match match in matches2)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            return translations;
        }

        private HashSet<string> GetTranslationsInTypescript(FileInfo file)
        {
            var content = File.ReadAllText(file.FullName);

            var translations = new HashSet<string>();

            var matches1 = Regex.Matches(content, "'i18n\\:(?<Key>[^\']+)'", RegexOptions.Singleline);

            foreach (Match match in matches1)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            var matches2 = Regex.Matches(content, "localizer.get\\('(?<Key>[^\']+)'\\)", RegexOptions.Singleline);

            foreach (Match match in matches2)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            var matches3 = Regex.Matches(content, "localizer.getOrKey\\('(?<Key>[^\']+)'\\)", RegexOptions.Singleline);

            foreach (Match match in matches3)
            {
                translations.Add(match.Groups["Key"].Value);
            }

            return translations;
        }
    }
}
