// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Web;

public sealed class ApiModelValidationAttribute : ActionFilterAttribute
{
    private const string RequestBodyTooLarge = "Request body too large.";
    private readonly bool allErrors;

    public ApiModelValidationAttribute(bool allErrors)
    {
        this.allErrors = allErrors;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = new List<ValidationError>();

        foreach (var (key, value) in context.ModelState)
        {
            if (value.ValidationState == ModelValidationState.Invalid)
            {
                foreach (var error in value.Errors)
                {
                    if (error.ErrorMessage?.Contains(RequestBodyTooLarge, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        throw new BadHttpRequestException(error.ErrorMessage, 413);
                    }
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add(new ValidationError(T.Get("common.httpInvalidRequestFormat")));
                }
                else
                {
                    var properties = Array.Empty<string>();

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        properties = new[] { key.ToCamelCase() };
                    }

                    foreach (var error in value.Errors)
                    {
                        if (!string.IsNullOrWhiteSpace(error.ErrorMessage) && allErrors)
                        {
                            errors.Add(new ValidationError(error.ErrorMessage, properties));
                        }
                        else if (error.Exception is JsonException jsonException)
                        {
                            errors.Add(new ValidationError(jsonException.Message));
                        }
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
