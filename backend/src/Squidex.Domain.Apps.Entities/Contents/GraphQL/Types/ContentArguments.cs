﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class ContentArguments
    {
        private static QueryArguments? query;

        public static readonly QueryArguments Find = new QueryArguments
        {
            new QueryArgument(AllTypes.None)
            {
                Name = "id",
                Description = "The id of the content (GUID).",
                DefaultValue = string.Empty,
                ResolvedType = AllTypes.NonNullGuid
            }
        };

        public static QueryArguments Query(int pageSize)
        {
            return query ??= new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "top",
                    Description = $"Optional number of contents to take (Default: {pageSize}).",
                    DefaultValue = pageSize,
                    ResolvedType = AllTypes.Int
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "skip",
                    Description = "Optional number of contents to skip.",
                    DefaultValue = 0,
                    ResolvedType = AllTypes.Int
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "filter",
                    Description = "Optional OData filter.",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.String
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "orderby",
                    Description = "Optional OData order definition.",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.String
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "search",
                    Description = "Optional OData full text search.",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.String
                },
            };
        }
    }
}
