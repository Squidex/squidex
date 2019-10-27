// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public static class AppEntityExtensions
    {
        public static PartitionResolver PartitionResolver(this IAppEntity entity)
        {
            return entity.LanguagesConfig.ToResolver();
        }
    }
}
