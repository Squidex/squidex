// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class EdmModelExtensions
    {
        static EdmModelExtensions()
        {
            CustomUriFunctions.AddCustomUriFunction("empty",
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(false),
                    EdmCoreModel.Instance.GetUntyped()));

            CustomUriFunctions.AddCustomUriFunction("exists",
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(false),
                    EdmCoreModel.Instance.GetUntyped()));

            CustomUriFunctions.AddCustomUriFunction("matchs",
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(false),
                    EdmCoreModel.Instance.GetString(false),
                    EdmCoreModel.Instance.GetString(false)));

            CustomUriFunctions.AddCustomUriFunction("distanceto",
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetDouble(false),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(true),
                    EdmCoreModel.Instance.GetInt32(true)));
        }

        public static ODataUriParser? ParseQuery(this IEdmModel model, string query)
        {
            if (!model.EntityContainer.EntitySets().Any())
            {
                return null;
            }

            query ??= string.Empty;

            var path = model.EntityContainer.EntitySets().First().Path.Path.Split('.')[^1];

            if (query.StartsWith('?'))
            {
                query = query[1..];
            }

            var parser = new ODataUriParser(model, new Uri($"{path}?{query}", UriKind.Relative));

            return parser;
        }

        public static ClrQuery ToQuery(this ODataUriParser? parser)
        {
            var query = new ClrQuery();

            if (parser != null)
            {
                parser.ParseTake(query);
                parser.ParseSkip(query);
                parser.ParseFilter(query);
                parser.ParseSort(query);
            }

            return query;
        }
    }
}
