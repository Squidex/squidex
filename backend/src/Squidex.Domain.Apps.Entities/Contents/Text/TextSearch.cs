// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed record TextQuery(string? Text, TextFilter? Filter)
    {
    }

    public sealed record TextFilter(DomainId[]? SchemaIds, bool Must)
    {
        public static TextFilter MustHaveSchemas(params DomainId[] schemaIds)
        {
            return new TextFilter(schemaIds, true);
        }

        public static TextFilter ShouldHaveSchemas(params DomainId[] schemaIds)
        {
            return new TextFilter(schemaIds, false);
        }
    }
}
