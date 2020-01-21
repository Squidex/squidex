// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
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

            var result = (ObjectResult)context.Result!;

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(400, (result.Value as ErrorDto)?.StatusCode);

            Assert.Equal(ex.Summary, (result.Value as ErrorDto)!.Message);

            Assert.Equal(new[] { "Error1", "P: Error2", "P1, P2: Error3" }, (result.Value as ErrorDto)!.Details);
        }

        [Fact]
        public void Should_generate_400_for_DomainException()
        {
            var context = E(new DomainException("NotAllowed"));

            sut.OnException(context);

            Validate(400, context.Result, context.Exception);
        }

        [Fact]
        public void Should_generate_412_for_DomainObjectVersionException()
        {
            var context = E(new DomainObjectVersionException("1", typeof(object), 1, 2));

            sut.OnException(context);

            Validate(412, context.Result, context.Exception);
        }

        [Fact]
        public void Should_generate_403_for_DomainForbiddenException()
        {
            var context = E(new DomainForbiddenException("Forbidden"));

            sut.OnException(context);

            Validate(403, context.Result, context.Exception);
        }

        [Fact]
        public void Should_generate_403_for_SecurityException()
        {
            var context = E(new SecurityException("Forbidden"));

            sut.OnException(context);

            Validate(403, context.Result, context.Exception);
        }

        [Fact]
        public async Task Should_unify_errror()
        {
            var context = R(new ProblemDetails { Status = 403, Type = "type" });

            await sut.OnResultExecutionAsync(context, () => Task.FromResult(Result(context)));

            Validate(403, context.Result, null);
        }

        private static ResultExecutedContext Result(ResultExecutingContext context)
        {
            var actionContext = ActionContext();

            return new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), context.Result, context.Controller);
        }

        private static ResultExecutingContext R(ProblemDetails problem)
        {
            var actionContext = ActionContext();

            return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(problem) { StatusCode = problem.Status }, null);
        }

        private static ExceptionContext E(Exception exception)
        {
            var actionContext = ActionContext();

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }

        private static ActionContext ActionContext()
        {
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                FilterDescriptors = new List<FilterDescriptor>()
            });

            return actionContext;
        }

        private static void Validate(int statusCode, IActionResult actionResult, Exception? exception)
        {
            var result = (ObjectResult)actionResult;

            var error = (ErrorDto)result.Value;

            Assert.NotNull(error.Type);

            Assert.Equal(statusCode, result.StatusCode);
            Assert.Equal(statusCode, error.StatusCode);

            if (exception != null)
            {
                Assert.Equal(exception.Message, error.Message);
            }
        }
    }
}
