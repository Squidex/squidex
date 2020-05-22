// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public static class AppExtensions
    {
        public static NamedId<DomainId> NamedId(this IAppEntity app)
        {
            return new NamedId<DomainId>(app.Id, app.Name);
        }
    }
}
