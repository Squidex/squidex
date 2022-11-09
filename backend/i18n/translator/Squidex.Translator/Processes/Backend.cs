// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Translator.Processes;

public static class Backend
{
    public static DirectoryInfo GetFolder(DirectoryInfo folder)
    {
        return new DirectoryInfo(Path.Combine(folder.FullName, "backend", "src"));
    }

    public static IEnumerable<(FileInfo, string)> GetFiles(DirectoryInfo folder)
    {
        var files =
            folder.GetFiles(@"*.cs", SearchOption.AllDirectories).Union(
            folder.GetFiles(@"*.cshtml", SearchOption.AllDirectories));

        foreach (var file in files)
        {
            var relativeName = Helper.RelativeName(file, folder);

            if (relativeName.Contains("/obj/", StringComparison.Ordinal) ||
                relativeName.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            yield return (file, relativeName);
        }
    }

    public static IEnumerable<(FileInfo, string)> GetFilesCS(DirectoryInfo folder)
    {
        var files = folder.GetFiles(@"*AssetsController.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var relativeName = Helper.RelativeName(file, folder);

            if (relativeName.Contains("/obj/", StringComparison.Ordinal) ||
                relativeName.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            yield return (file, relativeName);
        }
    }
}
