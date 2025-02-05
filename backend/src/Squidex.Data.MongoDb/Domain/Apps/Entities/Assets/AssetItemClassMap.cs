// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Assets;

internal static class AssetItemClassMap
{
    public static void Register()
    {
        AppEntityClassMap.Register();

        BsonClassMap.TryRegisterClassMap<AssetItem>(cm =>
        {
            cm.MapProperty(x => x.ParentId)
                .SetElementName("pi")
                .SetIgnoreIfDefault(true);
        });
    }
}
