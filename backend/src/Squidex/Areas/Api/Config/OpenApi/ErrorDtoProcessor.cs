// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class ErrorDtoProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;

        void AddResponse(string code, string description)
        {
            if (!IsErrorCode(code))
            {
                return;
            }

            if (!operation.Responses.ContainsKey(code))
            {
                operation.Responses.Add(code, new OpenApiResponse
                {
                    Description = description
                });
            }
        }

        var responses =
            context.MethodInfo.GetXmlDocsElement(null)?
                .Nodes()
                .OfType<XElement>()
                .Where(x => x.Name == "response")
                .Where(x => x.Attribute("code") != null)
                ?? Enumerable.Empty<XElement>();

        foreach (var response in responses)
        {
            AddResponse(response.Attribute("code")!.Value, response.Value);
        }

        if (!string.Equals(context.OperationDescription.Method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            AddResponse("400", "Validation error.");
        }

        AddResponse("500", "Operation failed.");

        foreach (var (code, response) in operation.Responses)
        {
            if (response.Schema == null)
            {
                if (IsErrorCode(code) && code != "404")
                {
                    response.Schema = GetErrorSchema(context);
                }
            }
        }

        return true;
    }

    private static bool IsErrorCode(string code)
    {
        return !code.StartsWith("2", StringComparison.OrdinalIgnoreCase);
    }

    private static JsonSchema GetErrorSchema(OperationProcessorContext context)
    {
        var errorType = typeof(ErrorDto).ToContextualType();

        return context.SchemaGenerator.GenerateWithReference<JsonSchema>(errorType, context.SchemaResolver);
    }
}
