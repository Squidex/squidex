// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public class TranslateTypescript
{
    private readonly TranslationService service;
    private readonly DirectoryInfo folder;

    public TranslateTypescript(DirectoryInfo folder, TranslationService service)
    {
        this.folder = Frontend.GetFolder(folder);

        this.service = service;
    }

    public void Run()
    {
        foreach (var (file, relativeName) in Frontend.GetTypescriptFiles(folder))
        {
            var content = File.ReadAllText(file.FullName);

            var isReplaced = false;

            content = Regex.Replace(content, "'[^']*'", match =>
            {
                var value = match.Value[1..^1];

                string result = null;

                if (value.IsPotentialMultiWordText())
                {
                    service.Translate(relativeName, value, "Code", key =>
                    {
                        result = $"\'i18n:{key}\'";

                        isReplaced = true;
                    });
                }

                return result ?? $"'{value}'";
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
