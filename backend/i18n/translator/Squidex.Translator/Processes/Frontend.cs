// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace Squidex.Translator.Processes
{
    public static class Frontend
    {
        public static DirectoryInfo GetFolder(DirectoryInfo folder)
        {
            return new DirectoryInfo(Path.Combine(folder.FullName, "frontend", "app"));
        }

        public static IEnumerable<(FileInfo, string)> GetTypescriptFiles(DirectoryInfo folder)
        {
            var files = folder.GetFiles(@"*.ts", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (file.Name.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return (file, Helper.RelativeName(file, folder));
            }
        }

        public static IEnumerable<(FileInfo, string)> GetTemplateFiles(DirectoryInfo folder)
        {
            var files = folder.GetFiles(@"*.html", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                yield return (file, Helper.RelativeName(file, folder));
            }
        }
    }
}
