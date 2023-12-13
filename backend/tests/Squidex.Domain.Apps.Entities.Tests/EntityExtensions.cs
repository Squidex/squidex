// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities;

public static class EntityExtensions
{
    public static App WithId(this App app, DomainId id)
    {
        return app with { Id = id };
    }

    public static Schema WithId(this Schema schema, NamedId<DomainId> id)
    {
        return schema.WithId(id.Id, id.Name);
    }

    public static Schema WithId(this Schema schema, DomainId id, string name)
    {
        return schema with { Id = id, Name = name };
    }

    public static Asset WithId<T>(this T asset, DomainId id) where T : Asset
    {
        return asset with { Id = id };
    }

    public static EnrichedAsset WithId(this EnrichedAsset content, DomainId id)
    {
        return content with { Id = id };
    }

    public static AssetFolder WithId(this AssetFolder folder, DomainId id)
    {
        return folder with { Id = id };
    }

    public static Content WithId(this Content content, DomainId id)
    {
        return content with { Id = id };
    }

    public static EnrichedContent WithId(this EnrichedContent content, DomainId id)
    {
        return content with { Id = id };
    }

    public static WriteContent WithId(this WriteContent content, DomainId id)
    {
        return content with { Id = id };
    }
}
