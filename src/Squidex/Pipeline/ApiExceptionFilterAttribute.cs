// ==========================================================================
//  ApiExceptionFilterAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Controllers.Api;
using Squidex.Infrastructure;

namespace Squidex.Pipeline
{
    public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter
    {
        private static readonly List<Func<Exception, IActionResult>> Handlers = new List<Func<Exception, IActionResult>>();

        private static void AddHandler<T>(Func<T, IActionResult> handler) where T : Exception
        {
            Handlers.Add(ex => ex is T typed ? handler(typed) : null);
        }

        static ApiExceptionFilterAttribute()
        {
            AddHandler<DomainObjectNotFoundException>(OnDomainObjectNotFoundException);
            AddHandler<DomainObjectVersionException>(OnDomainObjectVersionException);
            AddHandler<DomainForbiddenException>(OnDomainForbiddenException);
            AddHandler<DomainException>(OnDomainException);
            AddHandler<ValidationException>(OnValidationException);
        }

        private static IActionResult OnDomainObjectNotFoundException(DomainObjectNotFoundException ex)
        {
            return new NotFoundResult();
        }

        private static IActionResult OnDomainObjectVersionException(DomainObjectVersionException ex)
        {
            return ErrorResult(412, new ErrorDto { Message = ex.Message });
        }

        private static IActionResult OnDomainException(DomainException ex)
        {
            return ErrorResult(400, new ErrorDto { Message = ex.Message });
        }

        private static IActionResult OnDomainForbiddenException(DomainForbiddenException ex)
        {
            return ErrorResult(403, new ErrorDto { Message = ex.Message });
        }

        private static IActionResult OnValidationException(ValidationException ex)
        {
            return ErrorResult(400, new ErrorDto { Message = ex.Message, Details = ex.Errors.Select(e => e.Message).ToArray() });
        }

        private static IActionResult ErrorResult(int statusCode, ErrorDto error)
        {
            error.StatusCode = statusCode;

            return new ObjectResult(error) { StatusCode = statusCode };
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors =
                    context.ModelState.SelectMany(m =>
                        {
                            return m.Value.Errors.Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                                .Select(e => new ValidationError(e.ErrorMessage, m.Key));
                        }).ToList();

                throw new ValidationException("The model is not valid.", errors);
            }
        }

        public void OnException(ExceptionContext context)
        {
            IActionResult result = null;

            foreach (var handler in Handlers)
            {
                result = handler(context.Exception);

                if (result != null)
                {
                    break;
                }
            }

            if (result != null)
            {
                context.Result = result;
            }
        }
    }
}
