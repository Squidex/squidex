// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core;

namespace Squidex.Domain.Apps.Entities;

public static class MongoAppEntityClassMap
{
    public static void RegisterClassMap()
    {
        BsonClassMap.RegisterClassMap<AppEntity>(cm =>
        {
            cm.MapProperty(x => x.AppId)
                .SetElementName("ai")
                .SetIsRequired(true);

            cm.MapProperty(x => x.IsDeleted)
                .SetElementName("dl")
                .SetIgnoreIfDefault(false);
        });
    }
}
