// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Squidex.Infrastructure
{
    public static class FileExtensions
    {
        private static readonly string[] Extensions =
        {
            "bytes",
            "kB",
            "MB",
            "GB",
            "TB"
        };

        private static readonly Dictionary<string, string> UnifiedExtensions = new Dictionary<string, string>
        {
            ["jpeg"] = "jpg"
        };

        public static string FileType(this string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                var fileType = fileInfo.Extension[1..].ToLowerInvariant();

                if (UnifiedExtensions.TryGetValue(fileType, out var unified))
                {
                    return unified;
                }
                else
                {
                    return fileType;
                }
            }
            catch
            {
                return "blob";
            }
        }

        public static string ToReadableSize(this int value)
        {
            return ToReadableSize((long)value);
        }

        public static string ToReadableSize(this long value)
        {
            if (value < 0)
            {
                return string.Empty;
            }

            var d = (double)value;
            var u = 0;

            const int multiplier = 1024;

            while ((d >= multiplier || -d >= multiplier) && u < Extensions.Length - 1)
            {
                d /= multiplier;
                u++;
            }

            if (u >= Extensions.Length - 1)
            {
                u = Extensions.Length - 1;
            }

            return $"{Math.Round(d, 1).ToString(CultureInfo.InvariantCulture)} {Extensions[u]}";
        }
    }
}
