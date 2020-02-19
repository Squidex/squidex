// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class SearchFilter
    {
        public IReadOnlyList<Guid> SchemaIds { get; }

        public bool Must { get; }

        public SearchFilter(IReadOnlyList<Guid> schemaIds, bool must)
        {
            Guard.NotNull(schemaIds);

            SchemaIds = schemaIds;

            Must = must;
        }

        public static SearchFilter MustHaveSchemas(List<Guid> schemaIds)
        {
            return new SearchFilter(schemaIds, true);
        }

        public static SearchFilter MustHaveSchemas(params Guid[] schemaIds)
        {
            return new SearchFilter(schemaIds?.ToList()!, true);
        }

        public static SearchFilter ShouldHaveSchemas(params Guid[] schemaIds)
        {
            return new SearchFilter(schemaIds?.ToList()!, false);
        }
    }
}
