// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    internal static class AssetStoreHelper
    {
        public static string GetFileName(string id, long version, string? suffix = null)
        {
            Guard.NotNullOrEmpty(id);

            return StringExtensions.JoinNonEmpty("_", id, version.ToString(), suffix);
        }
    }
}
