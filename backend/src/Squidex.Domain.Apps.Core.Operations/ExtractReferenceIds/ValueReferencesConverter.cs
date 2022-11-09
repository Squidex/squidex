// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

public sealed class ValueReferencesConverter : IContentValueConverter
{
    private readonly HashSet<DomainId>? validIds;

    public ValueReferencesConverter(HashSet<DomainId>? validIds = null)
    {
        this.validIds = validIds;
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        if (validIds == null || source == default)
        {
            return (false, source);
        }

        return (false, ReferencesCleaner.Cleanup(field, source, validIds));
    }
}
