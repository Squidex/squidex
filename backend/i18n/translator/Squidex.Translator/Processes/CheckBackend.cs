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
    public class CheckBackend
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public CheckBackend(DirectoryInfo folder, TranslationService service)
        {
            this.folder = Backend.GetFolder(folder);

            this.service = service;
        }

        public void Run()
        {
            var all = new HashSet<string>();

            foreach (var (file, relativeName) in Backend.GetFiles(folder))
            {
                var content = File.ReadAllText(file.FullName);

                var matches = Regex.Matches(content, "T\\.Get\\(\"(?<Key>[^\"]*)\"");

                var translations = new HashSet<string>();

                foreach (Match match in matches)
                {
                    var key = match.Groups["Key"].Value;

                    translations.Add(key);

                    all.Add(key);
                }

                Helper.CheckForFile(service, relativeName, translations);
            }

            Helper.CheckUnused(service, all);
            Helper.CheckOtherLocales(service);

            service.Save();
        }
    }
}
