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
            this.folder = new DirectoryInfo(Path.Combine(folder.FullName, "backend", "src", "Squidex", "Areas", "Frontend", "Resources"));

            this.service = service;
        }

        public void Run()
        {
            var fullName = Path.Combine(folder.FullName, "texts.en");

            if (!folder.Exists)
            {
                Directory.CreateDirectory(folder.FullName);
            }

            service.WriteTexts(fullName);
            service.Save();
        }
    }
}
