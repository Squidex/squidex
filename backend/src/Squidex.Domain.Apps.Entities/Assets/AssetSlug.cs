// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets;

public static class AssetSlug
{
    private static readonly HashSet<char> Dot = new HashSet<char>(new[] { '.' });

    public static string ToAssetSlug(this string value)
    {
        return value.Slugify(Dot);
    }
}
