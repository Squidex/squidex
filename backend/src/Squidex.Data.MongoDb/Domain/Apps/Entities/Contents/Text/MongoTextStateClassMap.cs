// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Entities.Contents.Text.State;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

internal static class MongoTextStateClassMap
{
    public static void RegisterClassMap()
    {
        BsonClassMap.RegisterClassMap<TextContentState>(cm =>
        {
            cm.MapIdProperty(x => x.UniqueContentId);

            cm.MapProperty(x => x.State)
                .SetElementName("s");
        });
    }
}
