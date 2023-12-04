// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.MongoDb;

internal static class EntityClassMap
{
    public static void Register()
    {
        BsonClassMap.TryRegisterClassMap<Entity>(cm =>
        {
            cm.MapProperty(x => x.Id)
                .SetElementName("id")
                .SetIsRequired(true);

            cm.MapProperty(x => x.LastModified)
                .SetElementName("mt")
                .SetIsRequired(true);

            cm.MapProperty(x => x.LastModifiedBy)
                .SetElementName("mb")
                .SetIsRequired(true);

            cm.MapProperty(x => x.Created)
                .SetElementName("ct")
                .SetIsRequired(true);

            cm.MapProperty(x => x.CreatedBy)
                .SetElementName("cb")
                .SetIsRequired(true);

            cm.MapProperty(x => x.Version)
                .SetElementName("vs")
                .SetIsRequired(true);
        });
    }
}
