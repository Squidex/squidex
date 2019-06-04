// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public static class AppExtensions
    {
        public static NamedId<Guid> NamedId(this IAppEntity app)
        {
            return new NamedId<Guid>(app.Id, app.Name);
        }
    }
}
