// ==========================================================================
//  FileExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;

namespace Squidex.Infrastructure
{
    public static class FileExtensions
    {
        public static string FileType(this string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);

                return fileInfo.Extension.Substring(1).ToLowerInvariant();
            }
            catch
            {
                return "blob";
            }
        }
    }
}
