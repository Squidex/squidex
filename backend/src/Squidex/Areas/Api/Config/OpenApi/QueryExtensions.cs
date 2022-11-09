// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;
using Squidex.Domain.Apps.Core;

namespace Squidex.Areas.Api.Config.OpenApi;

public static class QueryExtensions
{
    public static void AddQuery(this OpenApiOperation operation, bool supportSearch)
    {
        var stringSchema = new JsonSchema
        {
            Type = JsonObjectType.String
        };

        var numberSchema = new JsonSchema
        {
            Type = JsonObjectType.Number
        };

        void AddParameterQuery(OpenApiParameter parameter)
        {
            if (operation.Parameters.Any(x => x.Name == parameter.Name && x.Kind == OpenApiParameterKind.Query))
            {
                return;
            }

            parameter.Kind = OpenApiParameterKind.Query;

            operation.Parameters.Add(parameter);
        }

        if (supportSearch)
        {
            AddParameterQuery(new OpenApiParameter
            {
                Schema = stringSchema,
                Name = "$search",
                Description = FieldDescriptions.QuerySkip
            });
        }

        AddParameterQuery(new OpenApiParameter
        {
            Schema = numberSchema,
            Name = "$top",
            Description = FieldDescriptions.QueryTop
        });

        AddParameterQuery(new OpenApiParameter
        {
            Schema = numberSchema,
            Name = "$skip",
            Description = FieldDescriptions.QuerySkip
        });

        AddParameterQuery(new OpenApiParameter
        {
            Schema = stringSchema,
            Name = "$orderby",
            Description = FieldDescriptions.QueryOrderBy
        });

        AddParameterQuery(new OpenApiParameter
        {
            Schema = stringSchema,
            Name = "$filter",
            Description = FieldDescriptions.QueryFilter
        });

        AddParameterQuery(new OpenApiParameter
        {
            Schema = stringSchema,
            Name = "q",
            Description = FieldDescriptions.QueryQ
        });

        AddParameterQuery(new OpenApiParameter
        {
            Schema = stringSchema,
            Name = "ids",
            Description = FieldDescriptions.QueryIds
        });
    }
}
