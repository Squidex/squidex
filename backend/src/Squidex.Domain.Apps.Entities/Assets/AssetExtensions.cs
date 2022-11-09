// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets;

public static class AssetExtensions
{
    private const string HeaderNoEnrichment = "X-NoAssetEnrichment";

    public static bool ShouldSkipAssetEnrichment(this Context context)
    {
        return context.Headers.ContainsKey(HeaderNoEnrichment);
    }

    public static ICloneBuilder WithoutAssetEnrichment(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(HeaderNoEnrichment, value);
    }
}
