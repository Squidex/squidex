// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public partial class TranslateBackend(DirectoryInfo folder, TranslationService service)
{
    private readonly DirectoryInfo folder = Backend.GetFolder(folder);

    public void Run()
    {
        foreach (var (file, relativeName) in Backend.GetFilesCS(folder))
        {
            var content = File.ReadAllText(file.FullName);

            var isReplaced = false;

            content = VariableRegex().Replace(content, match =>
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

    [GeneratedRegex("\"[^\"]*\"")]
    private static partial Regex VariableRegex();
}
