// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public sealed class GenerateFrontendResources
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public GenerateFrontendResources(DirectoryInfo folder, TranslationService service)
        {
            this.folder = new DirectoryInfo(Path.Combine(folder.FullName, "backend", "i18n"));

            this.service = service;
        }

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
}
