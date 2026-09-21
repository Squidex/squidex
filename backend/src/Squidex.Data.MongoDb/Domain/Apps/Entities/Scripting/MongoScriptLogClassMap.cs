// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Scripting;

internal static class MongoScriptLogClassMap
{
    public static void RegisterClassMap()
    {
        BsonClassMap.TryRegisterClassMap<ScriptLogEntry>(cm =>
        {
            cm.MapProperty(x => x.Timestamp)
                .SetElementName("t");

            cm.MapProperty(x => x.Level)
                .SetElementName("l");

            cm.MapProperty(x => x.Message)
                .SetElementName("m");

            cm.MapCreator(x => new ScriptLogEntry(x.Timestamp, x.Level, x.Message));
        });
    }
}
