// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Resources.NetStandard;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public sealed class GenerateBackendResources
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public GenerateBackendResources(DirectoryInfo folder, TranslationService service)
        {
            this.folder = new DirectoryInfo(Path.Combine(folder.FullName, "backend", "src", "Squidex.Shared"));

            this.service = service;
        }

        public void Run()
        {
            var fullName = Path.Combine(folder.FullName, "Texts.resx");

            using (var writer = new ResXResourceWriter(fullName))
            {
                foreach (var (key, value) in service.Texts)
                {
                    writer.AddResource(key, value);
                }
            }

            var text = File.ReadAllText(fullName);

            text = text.Replace("System.Resources.NetStandard.ResXResourceReader, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            text = text.Replace("System.Resources.NetStandard.ResXResourceWriter, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            File.WriteAllText(fullName, text);

            service.Save();
        }
    }
}
