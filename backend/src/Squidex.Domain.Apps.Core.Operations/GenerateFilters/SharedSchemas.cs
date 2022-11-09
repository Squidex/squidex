// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters;

internal static class SharedSchemas
{
    public static readonly FilterSchema Date = new FilterSchema(FilterSchemaType.DateTime)
    {
        Extra = new
        {
            editor = "Date"
        }
    };

    public static readonly FilterSchema DateTime = new FilterSchema(FilterSchemaType.DateTime)
    {
        Extra = new
        {
            editor = "DateTime"
        }
    };

    public static readonly FilterSchema Status = new FilterSchema(FilterSchemaType.String)
    {
        Extra = new
        {
            editor = "Status"
        }
    };

    public static readonly FilterSchema User = new FilterSchema(FilterSchemaType.String)
    {
        Extra = new
        {
            editor = "User"
        }
    };
}
