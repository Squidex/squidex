// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public static class DomainEntityExtensions
    {
        public static NamedId<Guid> NamedId(this IAppEntity entity)
        {
            return new NamedId<Guid>(entity.Id, entity.Name);
        }
    }
}
