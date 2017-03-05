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
using Squidex.Infrastructure;
using Squidex.Controllers.Api;

// ReSharper disable InvertIf

namespace Squidex.Pipeline
{
    public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter
    {
        private static readonly List<Func<Exception, IActionResult>> handlers = new List<Func<Exception, IActionResult>>();

        private static void AddHandler<T>(Func<T, IActionResult> handler) where T : Exception
        {
            handlers.Add(ex =>
            {
                var typed = ex as T;

                return typed != null ? handler(typed) : null;
            });
        }

        static ApiExceptionFilterAttribute()
        {
            AddHandler<DomainObjectNotFoundException>(OnDomainObjectNotFoundException);
            AddHandler<DomainObjectVersionException>(OnDomainObjectVersionException);
            AddHandler<DomainException>(OnDomainException);
            AddHandler<ValidationException>(OnValidationException);
        }

        private static IActionResult OnDomainObjectNotFoundException(DomainObjectNotFoundException ex)
        {
            return new NotFoundResult();
        }

        private static IActionResult OnDomainObjectVersionException(DomainObjectVersionException ex)
        {
            return new ObjectResult(new ErrorDto { Message = ex.Message }) { StatusCode = 412 };
        }

        private static IActionResult OnDomainException(DomainException ex)
        {
            return new BadRequestObjectResult(new ErrorDto { Message = ex.Message });
        }

        private static IActionResult OnValidationException(ValidationException ex)
        {
            return new BadRequestObjectResult(new ErrorDto { Message = ex.Message, Details = ex.Errors.Select(e => e.Message).ToArray() });
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.SelectMany(g => g.Errors).Select(e => new ValidationError(e.ErrorMessage)).ToList();

                throw new ValidationException("The model is not valid.", errors);
            }
        }

        public void OnException(ExceptionContext context)
        {
            IActionResult result = null;

            foreach (var handler in handlers)
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
