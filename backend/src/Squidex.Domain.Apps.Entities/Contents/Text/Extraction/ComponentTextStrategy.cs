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

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class ComponentTextStrategy : ITextFieldStrategy
{
    // Components are detected by value, because they can also be nested in other values.
    public int Order => 100;

    public bool TryExtract(IField? field, JsonValue value, ContentTextWalker walker)
    {
        if (!Component.IsValid(value, out var discriminator) || !walker.Context.Components.TryGetValue(DomainId.Create(discriminator), out var schema))
        {
            return false;
        }

        walker.AppendObject(value.AsObject, schema.FieldsByName);
        return true;
    }
}
