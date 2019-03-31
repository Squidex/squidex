// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Web
{
    public class ApiExceptionFilterAttributeTests
    {
        private readonly ApiExceptionFilterAttribute sut = new ApiExceptionFilterAttribute();

        [Fact]
        public void Should_generate_404_for_DomainObjectNotFoundException()
        {
            var context = E(new DomainObjectNotFoundException("1", typeof(object)));

            sut.OnException(context);

            Assert.IsType<NotFoundResult>(context.Result);
        }

        [Fact]
        public void Should_generate_400_for_ValidationException()
        {
            var ex = new ValidationException("NotAllowed",
                new ValidationError("Error1"),
                new ValidationError("Error2", "P"),
                new ValidationError("Error3", "P1", "P2"));

            var context = E(ex);

            sut.OnException(context);

            var result = context.Result as ObjectResult;

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(400, (result.Value as ErrorDto)?.StatusCode);

            Assert.Equal(ex.Summary, (result.Value as ErrorDto).Message);

            Assert.Equal(new[] { "Error1", "P: Error2", "P1, P2: Error3" }, (result.Value as ErrorDto).Details);
        }

        [Fact]
        public void Should_generate_400_for_DomainException()
        {
            var context = E(new DomainException("NotAllowed"));

            sut.OnException(context);

            Validate(400, context);
        }

        [Fact]
        public void Should_generate_412_for_DomainObjectVersionException()
        {
            var context = E(new DomainObjectVersionException("1", typeof(object), 1, 2));

            sut.OnException(context);

            Validate(412, context);
        }

        [Fact]
        public void Should_generate_403_for_DomainForbiddenException()
        {
            var context = E(new DomainForbiddenException("Forbidden"));

            sut.OnException(context);

            Validate(403, context);
        }

        [Fact]
        public void Should_generate_403_for_SecurityException()
        {
            var context = E(new SecurityException("Forbidden"));

            sut.OnException(context);

            Validate(403, context);
        }

        private static ExceptionContext E(Exception exception)
        {
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                FilterDescriptors = new List<FilterDescriptor>()
            });

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }

        private static void Validate(int statusCode, ExceptionContext context)
        {
            var result = context.Result as ObjectResult;

            Assert.Equal(statusCode, result.StatusCode);
            Assert.Equal(statusCode, (result.Value as ErrorDto)?.StatusCode);

            Assert.Equal(context.Exception.Message, (result.Value as ErrorDto).Message);
        }
    }
}
