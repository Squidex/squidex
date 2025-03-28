﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public partial class TranslateTypescript(DirectoryInfo folder, TranslationService service)
{
    private readonly DirectoryInfo folder = Frontend.GetFolder(folder);

    public void Run()
    {
        foreach (var (file, relativeName) in Frontend.GetTypescriptFiles(folder))
        {
            var content = File.ReadAllText(file.FullName);

            var isReplaced = false;

            content = TextRegex().Replace(content, match =>
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

    [GeneratedRegex("'[^']*'")]
    private static partial Regex TextRegex();
}
