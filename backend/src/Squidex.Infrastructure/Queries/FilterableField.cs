// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries
{
    public sealed record FilterableField(FilterableFieldType Type, string FieldPath)
    {
        public string? FieldDescription { get; init; }

        public string? TypeName { get; init; }

        public object? Extra { get; init; }

        public bool IsNullable { get; init; }

        public ReadonlyList<FilterableField>? Fields { get; init; }
    }
}
