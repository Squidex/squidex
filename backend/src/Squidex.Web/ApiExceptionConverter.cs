// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web
{
    public static class ApiExceptionConverter
    {
        private static readonly Dictionary<int, string> Links = new Dictionary<int, string>
        {
            [400] = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            [401] = "https://tools.ietf.org/html/rfc7235#section-3.1",
            [403] = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            [404] = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            [406] = "https://tools.ietf.org/html/rfc7231#section-6.5.6",
            [409] = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            [412] = "https://tools.ietf.org/html/rfc7231#section-6.5.10",
            [415] = "https://tools.ietf.org/html/rfc7231#section-6.5.13",
            [422] = "https://tools.ietf.org/html/rfc4918#section-11.2",
            [500] = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        public static (ErrorDto Error, bool WellKnown) ToErrorDto(this ProblemDetails problem, HttpContext? httpContext)
        {
            Guard.NotNull(problem);

            var error = new ErrorDto { Message = problem.Title, StatusCode = problem.Status };

            Enrich(httpContext, error);

            return (error, true);
        }

        public static (ErrorDto Error, bool WellKnown) ToErrorDto(this Exception exception, HttpContext? httpContext)
        {
            Guard.NotNull(exception);

            var result = CreateError(exception);

            Enrich(httpContext, result.Error);

            return result;
        }

        private static void Enrich(HttpContext? httpContext, ErrorDto error)
        {
            error.TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

            if (error.StatusCode.HasValue)
            {
                error.Type = Links.GetOrDefault(error.StatusCode.Value);
            }
        }

        private static (ErrorDto Error, bool WellKnown) CreateError(Exception exception)
        {
            switch (exception)
            {
                case ValidationException ex:
                    return (new ErrorDto
                    {
                        StatusCode = 400,
                        Message = ex.Summary,
                        Details = ToDetails(ex)
                    }, true);

                case DomainObjectNotFoundException _:
                    return (new ErrorDto
                    {
                        StatusCode = 404,
                        Message = null!
                    }, true);

                case DomainObjectVersionException _:
                    return (new ErrorDto
                    {
                        StatusCode = 412,
                        Message = exception.Message
                    }, true);

                case DomainForbiddenException _:
                    return (new ErrorDto
                    {
                        StatusCode = 403,
                        Message = exception.Message
                    }, true);

                case DomainException _:
                    return (new ErrorDto
                    {
                        StatusCode = 400,
                        Message = exception.Message
                    }, true);

                case SecurityException _:
                    return (new ErrorDto
                    {
                        StatusCode = 403,
                        Message = "Forbidden"
                    }, false);

                case DecoderFallbackException _:
                    return (new ErrorDto
                    {
                        StatusCode = 400,
                        Message = exception.Message
                    }, true);

                default:
                    return (new ErrorDto
                    {
                        StatusCode = 500,
                        Message = "Server Error"
                    }, false);
            }
        }

        private static string[] ToDetails(ValidationException ex)
        {
            return ex.Errors.Select(e =>
            {
                if (e.PropertyNames?.Any() == true)
                {
                    return $"{string.Join(", ", e.PropertyNames)}: {e.Message}";
                }
                else
                {
                    return e.Message;
                }
            }).ToArray();
        }
    }
}
