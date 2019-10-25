﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
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

                foreach (var m in context.ModelState)
                {
                    foreach (var e in m.Value.Errors)
                    {
                        if (!string.IsNullOrWhiteSpace(e.ErrorMessage) && (allErrors || e.Exception is JsonException))
                        {
                            errors.Add(new ValidationError(e.ErrorMessage, m.Key));
                        }
                        else if (e.Exception is JsonException jsonException)
                        {
                            errors.Add(new ValidationError(jsonException.Message));
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    throw new ValidationException("The model is not valid.", errors);
                }
            }
        }
    }
}
