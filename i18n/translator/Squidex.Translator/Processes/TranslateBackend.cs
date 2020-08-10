// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes
{
    public class TranslateBackend
    {
        private readonly TranslationService service;
        private readonly DirectoryInfo folder;

        public TranslateBackend(DirectoryInfo folder, TranslationService service)
        {
            this.folder = Backend.GetFolder(folder);

            this.service = service;
        }

        public void Run()
        {
            foreach (var (file, relativeName) in Backend.GetFilesCS(folder))
            {
                var content = File.ReadAllText(file.FullName);

                var isReplaced = false;

                content = Regex.Replace(content, "\"[^\"]*\"", match =>
                {
                    var value = match.Value[1..^1];

                    string result = null;

                    if (value.IsPotentialMultiWordText())
                    {
                        service.Translate(relativeName, value, "Code", key =>
                        {
                            result = $"T.Get(\"{key}\")";

                            isReplaced = true;
                        });
                    }

                    return result ?? $"\"{value}\"";
                });

                if (isReplaced)
                {
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("FILE {0} done", relativeName);

                    File.WriteAllText(file.FullName, content);
                }
            }
        }
    }
}
