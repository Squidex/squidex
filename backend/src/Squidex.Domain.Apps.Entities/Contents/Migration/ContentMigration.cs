// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Migration;

public static class ContentMigration
{
    public static ContentConverter CreateConverter(Schema schema, ResolvedComponents components, LanguagesConfig languages, IJsonSerializer serializer)
    {
        // The converter itself removes all fields and components that are not part of the schema anymore.
        var converter = new ContentConverter(components, schema);

        // Remove all values that are not compatible with the current field type.
        converter.Add(new ExcludeChangedTypes(serializer));

        // Move the values over when the partitioning of a field has been changed.
        converter.Add(new ResolveFromPreviousPartitioning(languages));

        return converter;
    }
}
