// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

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

    private static HashSet<string> GetTranslationsInTemplate(FileInfo file)
    {
        var content = File.ReadAllText(file.FullName);

        var translations = new HashSet<string>();

        void AddTranslations(string regex)
        {
            var matches = Regex.Matches(content, regex, RegexOptions.Singleline | RegexOptions.ExplicitCapture);

            foreach (Match match in matches)
            {
                translations.Add(match.Groups["Key"].Value);
            }
        }

        AddTranslations("\"i18n\\:(?<Key>[^\"]+)\"");
        AddTranslations("'i18n\\:(?<Key>[^\']+)'");
        AddTranslations("'(?<Key>[^\']+)' \\| sqxTranslate");

        return translations;
    }

    private static HashSet<string> GetTranslationsInTypescript(FileInfo file)
    {
        var content = File.ReadAllText(file.FullName);

        var translations = new HashSet<string>();

        void AddTranslations(string regex)
        {
            var matches = Regex.Matches(content, regex, RegexOptions.Singleline | RegexOptions.ExplicitCapture);

            foreach (Match match in matches)
            {
                translations.Add(match.Groups["Key"].Value);
            }
        }

        AddTranslations("'i18n\\:(?<Key>[^\']+)'");
        AddTranslations("localizer.get\\('(?<Key>[^\']+)'\\)");
        AddTranslations("localizer.getOrKey\\('(?<Key>[^\']+)'\\)");

        return translations;
    }
}
