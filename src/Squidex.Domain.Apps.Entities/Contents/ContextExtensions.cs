// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents
{
    public static class ContextExtensions
    {
        private static readonly char[] Separators = { ',', ';' };

        public static bool IsUnpublished(this Context context)
        {
            return context.Headers.ContainsKey("X-Unpublished");
        }

        public static Context WithUnpublished(this Context context)
        {
            context.Headers["X-Unpublished"] = "1";

            return context;
        }

        public static bool IsFlatten(this Context context)
        {
            return context.Headers.ContainsKey("X-Flatten");
        }

        public static Context WithFlatten(this Context context)
        {
            context.Headers["X-Flatten"] = "1";

            return context;
        }

        public static bool IsResolveFlow(this Context context)
        {
            return context.Headers.ContainsKey("X-ResolveFlow");
        }

        public static Context WithResolveFlow(this Context context)
        {
            context.Headers["X-ResolveFlow"] = "1";

            return context;
        }
    }
}
