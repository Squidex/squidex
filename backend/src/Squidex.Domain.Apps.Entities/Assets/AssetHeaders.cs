// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets;

public static class AssetHeaders
{
    public const string NoEnrichment = "X-NoAssetEnrichment";

    public static bool NoAssetEnrichment(this Context context)
    {
        return context.AsBoolean(NoEnrichment);
    }

    public static ICloneBuilder WithNoAssetEnrichment(this ICloneBuilder builder, bool value = true)
    {
        return builder.WithBoolean(NoEnrichment, value);
    }
}
