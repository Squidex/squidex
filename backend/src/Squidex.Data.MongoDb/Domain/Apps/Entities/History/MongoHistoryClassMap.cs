// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;

namespace Squidex.Domain.Apps.Entities.History;

internal static class MongoHistoryClassMap
{
    public static void RegisterClassMap()
    {
        BsonClassMap.RegisterClassMap<HistoryEvent>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.OwnerId)
                .SetElementName("AppId");

            cm.MapProperty(x => x.EventType)
                .SetElementName("Message");
        });
    }
}
