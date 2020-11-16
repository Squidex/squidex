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
using Squidex.Infrastructure.Translations;
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

        public static (ErrorDto Error, bool WellKnown) ToErrorDto(int statusCode, HttpContext? httpContext)
        {
            var error = new ErrorDto { StatusCode = statusCode };

            Enrich(httpContext, error);

            return (error, true);
        }

        public static (ErrorDto Error, bool WellKnown) ToErrorDto(this ProblemDetails problem, HttpContext? httpContext)
        {
            Guard.NotNull(problem, nameof(problem));

            var error = CreateError(problem.Status ?? 500, problem.Title);

            Enrich(httpContext, error);

            return (error, true);
        }

        public static (ErrorDto Error, bool WellKnown) ToErrorDto(this Exception exception, HttpContext? httpContext)
        {
            Guard.NotNull(exception, nameof(exception));

            var result = CreateError(exception);

            Enrich(httpContext, result.Error);

            return result;
        }

        private static void Enrich(HttpContext? httpContext, ErrorDto error)
        {
            error.TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

            if (error.StatusCode == 0)
            {
                error.StatusCode = 500;
            }

            error.Type = Links.GetOrDefault(error.StatusCode);
        }

        private static (ErrorDto Error, bool WellKnown) CreateError(Exception exception)
        {
            switch (exception)
            {
                case ValidationException ex:
                    return (CreateError(400, T.Get("common.httpValidationError"), ToDetails(ex)), true);

                case DomainObjectNotFoundException _:
                    return (CreateError(404), true);

                case DomainObjectVersionException _:
                    return (CreateError(412, exception.Message), true);

                case DomainObjectConflictException _:
                    return (CreateError(409, exception.Message), true);

                case DomainForbiddenException _:
                    return (CreateError(403, exception.Message), true);

                case DomainException _:
                    return (CreateError(400, exception.Message), true);

                case SecurityException _:
                    return (CreateError(403), false);

                case DecoderFallbackException _:
                    return (CreateError(400, exception.Message), true);

                case BadHttpRequestException ex:
                    return (CreateError(ex.StatusCode, ex.Message), true);

                default:
                    return (CreateError(500), false);
            }
        }

        private static ErrorDto CreateError(int status, string? message = null, string[]? details = null)
        {
            var error = new ErrorDto { StatusCode = status, Message = message, Details = details };

            return error;
        }

        private static string[] ToDetails(ValidationException ex)
        {
            static string FixPropertyName(string property)
            {
                property = property.Trim();

                if (property.Length == 0)
                {
                    return property;
                }

                var prevChar = 0;

                var builder = new StringBuilder(property.Length);

                builder.Append(char.ToLower(property[0]));

                foreach (var character in property.Skip(1))
                {
                    if (prevChar == '.')
                    {
                        builder.Append(char.ToLower(character));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    prevChar = character;
                }

                return builder.ToString();
            }

            return ex.Errors.Select(e =>
            {
                if (e.PropertyNames?.Any() == true)
                {
                    return $"{string.Join(", ", e.PropertyNames.Select(FixPropertyName))}: {e.Message}";
                }
                else
                {
                    return e.Message;
                }
            }).ToArray();
        }
    }
}
