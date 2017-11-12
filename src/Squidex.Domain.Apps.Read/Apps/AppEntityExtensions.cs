// ==========================================================================
//  AppEntityExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core;

namespace Squidex.Domain.Apps.Read.Apps
{
    public static class AppEntityExtensions
    {
        public static PartitionResolver PartitionResolver(this IAppEntity entity)
        {
            return entity.LanguagesConfig.ToResolver();
        }
    }
}
