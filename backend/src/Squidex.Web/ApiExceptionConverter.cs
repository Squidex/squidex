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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web
{
    public static class ApiExceptionConverter
    {
        private static readonly List<Func<Exception, ErrorDto?>> Handlers = new List<Func<Exception, ErrorDto?>>();
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
            [500] = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };

        private static void AddHandler<T>(Func<T, ErrorDto> handler) where T : Exception
        {
            Handlers.Add(ex => ex is T typed ? handler(typed) : null);
        }

        static ApiExceptionConverter()
        {
            AddHandler<ValidationException>(OnValidationException);
            AddHandler<DecoderFallbackException>(OnDecoderException);
            AddHandler<DomainObjectNotFoundException>(OnDomainObjectNotFoundException);
            AddHandler<DomainObjectVersionException>(OnDomainObjectVersionException);
            AddHandler<DomainForbiddenException>(OnDomainForbiddenException);
            AddHandler<DomainException>(OnDomainException);
            AddHandler<SecurityException>(OnSecurityException);
        }

        public static ErrorDto ToErrorDto(this Exception exception, HttpContext? httpContext)
        {
            Guard.NotNull(exception);

            ErrorDto? result = null;

            foreach (var handler in Handlers)
            {
                result = handler(exception);

                if (result != null)
                {
                    result.TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

                    if (result.StatusCode.HasValue)
                    {
                        result.Type = Links.GetOrDefault(result.StatusCode.Value);
                    }

                    return result;
                }
            }

            return new ErrorDto { StatusCode = 500 };
        }

        private static ErrorDto OnDecoderException(DecoderFallbackException ex)
        {
            return new ErrorDto { StatusCode = 400, Message = ex.Message };
        }

        private static ErrorDto OnDomainObjectNotFoundException(DomainObjectNotFoundException ex)
        {
            return new ErrorDto { StatusCode = 404 };
        }

        private static ErrorDto OnDomainObjectVersionException(DomainObjectVersionException ex)
        {
            return new ErrorDto { StatusCode = 412, Message = ex.Message };
        }

        private static ErrorDto OnDomainException(DomainException ex)
        {
            return new ErrorDto { StatusCode = 400, Message = ex.Message };
        }

        private static ErrorDto OnDomainForbiddenException(DomainForbiddenException ex)
        {
            return new ErrorDto { StatusCode = 403, Message = ex.Message };
        }

        private static ErrorDto OnSecurityException(SecurityException ex)
        {
            return new ErrorDto { StatusCode = 403, Message = ex.Message };
        }

        private static ErrorDto OnValidationException(ValidationException ex)
        {
            return new ErrorDto { StatusCode = 400, Message = ex.Summary, Details = ToDetails(ex) };
        }

        private static string[] ToDetails(ValidationException ex)
        {
            return ex.Errors?.Select(e =>
            {
                if (e.PropertyNames?.Any() == true)
                {
                    return $"{string.Join(", ", e.PropertyNames)}: {e.Message}";
                }
                else
                {
                    return e.Message;
                }
            }).ToArray() ?? new string[0];
        }
    }
}
