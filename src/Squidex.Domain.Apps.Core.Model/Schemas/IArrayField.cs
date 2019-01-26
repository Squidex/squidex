// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IArrayField : IField<ArrayFieldProperties>
    {
        IReadOnlyList<NestedField> Fields { get; }

        IReadOnlyDictionary<long, NestedField> FieldsById { get; }

        IReadOnlyDictionary<string, NestedField> FieldsByName { get; }

        FieldCollection<NestedField> FieldCollection { get; }
    }
}
