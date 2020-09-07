// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web
{
    public sealed class ApiModelValidationAttribute : ActionFilterAttribute
    {
        private readonly bool allErrors;

        public ApiModelValidationAttribute(bool allErrors)
        {
            this.allErrors = allErrors;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new List<ValidationError>();

                foreach (var (key, value) in context.ModelState)
                {
                    if (value.ValidationState == ModelValidationState.Invalid)
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            errors.Add(new ValidationError(T.Get("common.httpInvalidRequestFormat")));
                        }
                        else
                        {
                            foreach (var error in value.Errors)
                            {
                                if (!string.IsNullOrWhiteSpace(error.ErrorMessage) && ShouldExpose(error))
                                {
                                    errors.Add(new ValidationError(error.ErrorMessage));
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

        private bool ShouldExpose(ModelError error)
        {
            return allErrors || error.Exception is JsonException;
        }
    }
}
