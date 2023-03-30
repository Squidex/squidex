// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class AddSchemaNames : IContentItemConverter
{
    private readonly ResolvedComponents components;

    public AddSchemaNames(ResolvedComponents components)
    {
        this.components = components;
    }

    public JsonObject ConvertItemBefore(IField parentField, JsonObject item, IEnumerable<IField> schema)
    {
        if (parentField is IArrayField)
        {
            return item;
        }

        if (item.ContainsKey("schemaName"))
        {
            return item;
        }

        if (!Component.IsValid(item, out var discriminator))
        {
            return item;
        }

        var id = DomainId.Create(discriminator);

        if (components.TryGetValue(id, out var component))
        {
            item["schemaName"] = component.Name;
        }

        return item;
    }
}
