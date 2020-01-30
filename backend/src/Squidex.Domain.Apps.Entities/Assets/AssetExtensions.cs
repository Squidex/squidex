// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets
{
    public static class AssetExtensions
    {
        private const string HeaderNoEnrichment = "X-NoAssetEnrichment";

        public static bool ShouldEnrichAsset(this Context context)
        {
            return !context.Headers.ContainsKey(HeaderNoEnrichment);
        }

        public static Context WithoutAssetEnrichment(this Context context, bool value = true)
        {
            if (value)
            {
                context.Headers[HeaderNoEnrichment] = "1";
            }
            else
            {
                context.Headers.Remove(HeaderNoEnrichment);
            }

            return context;
        }
    }
}
