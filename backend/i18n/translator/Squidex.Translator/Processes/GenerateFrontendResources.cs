// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public sealed class GenerateFrontendResources(DirectoryInfo folder, TranslationService service)
{
    private readonly DirectoryInfo folder = new DirectoryInfo(Path.Combine(folder.FullName, "backend", "i18n"));

    public void Run()
    {
        foreach (var locale in service.SupportedLocales)
        {
            var fullName = Path.Combine(folder.FullName, $"frontend_{locale}.json");

            if (!folder.Exists)
            {
                Directory.CreateDirectory(folder.FullName);
            }

            var texts = service.GetTextsWithFallback(locale);

            service.WriteTo(texts, fullName);
        }

        service.Save();
    }
}
