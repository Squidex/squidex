// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Resources.NetStandard;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public sealed class GenerateKeys
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public GenerateKeys(DirectoryInfo folder, TranslationService service, string type)
        {
            if (type == "frontend")
            {
                this.folder = Frontend.GetFolder(folder);
            }
            else if (type == "backend")
            {
                this.folder = new DirectoryInfo(Path.Combine(folder.FullName, "backend", "src", "Squidex.Shared"));
            }

            this.service = service;
        }

        public void Run()
        {
            service.SaveKeys();
        }
    }
}
