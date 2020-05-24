// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class SearchFilter
    {
        public IReadOnlyList<DomainId> SchemaIds { get; }

        public bool Must { get; }

        public SearchFilter(IReadOnlyList<DomainId> schemaIds, bool must)
        {
            Guard.NotNull(schemaIds, nameof(schemaIds));

            SchemaIds = schemaIds;

            Must = must;
        }

        public static SearchFilter MustHaveSchemas(List<DomainId> schemaIds)
        {
            return new SearchFilter(schemaIds, true);
        }

        public static SearchFilter MustHaveSchemas(params DomainId[] schemaIds)
        {
            return new SearchFilter(schemaIds?.ToList()!, true);
        }

        public static SearchFilter ShouldHaveSchemas(params DomainId[] schemaIds)
        {
            return new SearchFilter(schemaIds?.ToList()!, false);
        }
    }
}
