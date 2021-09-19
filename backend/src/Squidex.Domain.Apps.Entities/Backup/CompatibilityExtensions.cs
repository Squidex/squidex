// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public static class CompatibilityExtensions
    {
        private const string VersionFile = "Version.json";
        private static readonly FileVersion None = new FileVersion();
        private static readonly FileVersion Expected = new FileVersion { Major = 5 };

#pragma warning disable MA0077 // A class that provides Equals(T) should implement IEquatable<T>
        public sealed class FileVersion
#pragma warning restore MA0077 // A class that provides Equals(T) should implement IEquatable<T>
        {
            public int Major { get; set; }

            public bool Equals(FileVersion other)
            {
                return Major == other.Major;
            }
        }

        public static Task WriteVersionAsync(this IBackupWriter writer)
        {
            return writer.WriteJsonAsync(VersionFile, Expected);
        }

        public static async Task CheckCompatibilityAsync(this IBackupReader reader)
        {
            var current = await reader.ReadVersionAsync();

            if (None.Equals(current))
            {
                return;
            }

            if (!Expected.Equals(current))
            {
                throw new BackupRestoreException("Backup file is not compatible with this version.");
            }
        }

        private static async Task<FileVersion> ReadVersionAsync(this IBackupReader reader)
        {
            try
            {
                return await reader.ReadJsonAsync<FileVersion>(VersionFile);
            }
            catch
            {
                return None;
            }
        }
    }
}
