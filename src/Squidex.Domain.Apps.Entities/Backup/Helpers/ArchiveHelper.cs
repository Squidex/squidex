// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Backup.Helpers
{
    public static class ArchiveHelper
    {
        private const int MaxAttachmentFolders = 1000;
        private const int MaxEventsPerFolder = 1000;

        public static string GetAttachmentPath(string name)
        {
            name = name.ToLowerInvariant();

            var attachmentFolder = SimpleHash(name) % MaxAttachmentFolders;
            var attachmentPath = $"attachments/{attachmentFolder}/{name}";

            return attachmentPath;
        }

        public static string GetEventPath(int index)
        {
            var eventFolder = index / MaxEventsPerFolder;
            var eventPath = $"events/{eventFolder}/{index}.json";

            return eventPath;
        }

        private static int SimpleHash(string value)
        {
            var hash = 17;

            foreach (var c in value)
            {
                unchecked
                {
                    hash = (hash * 23) + c.GetHashCode();
                }
            }

            return Math.Abs(hash);
        }
    }
}
