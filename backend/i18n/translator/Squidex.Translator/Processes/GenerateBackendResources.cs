// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Resources.NetStandard;
using System.Text.RegularExpressions;
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
            foreach (var locale in service.SupportedLocales)
            {
                var name = locale ==
                    service.MainLocale ?
                        $"Texts.resx" :
                        $"Texts.{locale}.resx";

                var fullName = Path.Combine(folder.FullName, name);

                using (var writer = new ResXResourceWriter(fullName))
                {
                    var texts = service.GetTextsWithFallback(locale);

                    foreach (var (key, value) in texts)
                    {
                        writer.AddResource(key, value);

                        if (key.StartsWith("annotations_", StringComparison.OrdinalIgnoreCase))
                        {
                            var i = 0;

                            var dotnetKey = $"dotnet_{key}";
                            var dotnetValue = Regex.Replace(value, "{[^}]*}", m => $"{{{i++}}}");

                            writer.AddResource(dotnetKey, dotnetValue);
                        }
                    }
                }

                var text = File.ReadAllText(fullName);

                text = text.Replace("System.Resources.NetStandard.ResXResourceReader, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                text = text.Replace("System.Resources.NetStandard.ResXResourceWriter, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

                File.WriteAllText(fullName, text);
            }

            service.Save();
        }
    }
}
