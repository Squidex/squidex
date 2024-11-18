// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public sealed class GenerateKeys(DirectoryInfo folder, TranslationService service, string fileName)
{
    public void Run()
    {
        var keys = new TranslatedTexts();

        foreach (var text in service.MainTranslations)
        {
            keys.Add(text.Key, string.Empty);
        }

        var fullName = Path.Combine(folder.FullName, fileName);

        if (!folder.Exists)
        {
            Directory.CreateDirectory(folder.FullName);
        }

        service.WriteTo(keys, fullName);

        service.Save();
    }
}
