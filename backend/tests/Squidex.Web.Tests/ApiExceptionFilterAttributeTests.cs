// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Xunit;

#pragma warning disable MA0015 // Specify the parameter name in ArgumentException

namespace Squidex.Web
{
    public class ApiExceptionFilterAttributeTests
    {
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly ApiExceptionFilterAttribute sut = new ApiExceptionFilterAttribute();

        [Fact]
        public void Should_generate_400_for_ValidationException()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("Error1"),
                new ValidationError("Error2", "Property0"),
                new ValidationError("Error3", "Property1", "Property2"),
                new ValidationError("Error4", "Property3.Property4"),
                new ValidationError("Error5", "Property5[0].Property6")
            };

            var ex = new ValidationException(errors);

            var context = Error(ex);

            sut.OnException(context);

            var result = (ObjectResult)context.Result!;

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(400, (result.Value as ErrorDto)?.StatusCode);

            Assert.Equal(new[]
            {
                "Error1",
                "property0: Error2",
                "property1, property2: Error3",
                "property3.property4: Error4",
                "property5[0].property6: Error5"
            }, ((ErrorDto)result.Value).Details);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_404_for_DomainObjectNotFoundException()
        {
            var context = Error(new DomainObjectNotFoundException("1"));

            sut.OnException(context);

            Assert.IsType<NotFoundResult>(context.Result);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_500_and_log_for_unknown_error()
        {
            var context = Error(new InvalidOperationException());

            sut.OnException(context);

            Validate(500, context.Result, null);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, context.Exception, A<LogFormatter>._!))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_generate_400_for_DomainException()
        {
            var context = Error(new DomainException("NotAllowed"));

            sut.OnException(context);

            Validate(400, context.Result, context.Exception);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_400_for_DomainException_with_error_code()
        {
            var context = Error(new DomainException("NotAllowed", "ERROR_CODE_XYZ"));

            sut.OnException(context);

            Validate(400, context.Result, context.Exception, "ERROR_CODE_XYZ");

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_400_for_DecoderFallbackException()
        {
            var context = Error(new DecoderFallbackException("Decoder"));

            sut.OnException(context);

            Validate(400, context.Result, context.Exception);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_409_for_DomainObjectConflictException()
        {
            var context = Error(new DomainObjectConflictException("1"));

            sut.OnException(context);

            Validate(409, context.Result, context.Exception, "OBJECT_CONFLICT");

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_410_for_DomainObjectDeletedException()
        {
            var context = Error(new DomainObjectDeletedException("1"));

            sut.OnException(context);

            Validate(410, context.Result, context.Exception, "OBJECT_DELETED");

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_412_for_DomainObjectVersionException()
        {
            var context = Error(new DomainObjectVersionException("1", 1, 2));

            sut.OnException(context);

            Validate(412, context.Result, context.Exception, "OBJECT_VERSION_CONFLICT");

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_403_for_DomainForbiddenException()
        {
            var context = Error(new DomainForbiddenException("Forbidden"));

            sut.OnException(context);

            Validate(403, context.Result, context.Exception, "FORBIDDEN");

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_generate_403_and_log_for_SecurityException()
        {
            var context = Error(new SecurityException());

            sut.OnException(context);

            Validate(403, context.Result, null);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, context.Exception, A<LogFormatter>._!))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_unify_errror()
        {
            var context = Problem(new ProblemDetails { Status = 403, Type = "type" });

            await sut.OnResultExecutionAsync(context, () => Task.FromResult(Result(context)));

            Validate(403, context.Result, null);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        private ResultExecutedContext Result(ResultExecutingContext context)
        {
            var actionContext = ActionContext();

            var result = context.Result;

            return new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), result, context.Controller);
        }

        private ResultExecutingContext Problem(ProblemDetails problem)
        {
            var actionContext = ActionContext();

            var result = new ObjectResult(problem) { StatusCode = problem.Status };

            return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, null!);
        }

        private ExceptionContext Error(Exception exception)
        {
            var actionContext = ActionContext();

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }

        private ActionContext ActionContext()
        {
            var services = A.Fake<IServiceProvider>();

            A.CallTo(() => services.GetService(typeof(ISemanticLog)))
                .Returns(log);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                FilterDescriptors = new List<FilterDescriptor>()
            });

            return actionContext;
        }

        private static void Validate(int statusCode, IActionResult? actionResult, Exception? exception, string? errorCode = null)
        {
            var result = actionResult as ObjectResult;

            var error = result?.Value as ErrorDto;

            Assert.NotNull(error?.Type);

            Assert.Equal(statusCode, result?.StatusCode);
            Assert.Equal(statusCode, error?.StatusCode);
            Assert.Equal(errorCode, error?.ErrorCode);

            if (exception != null)
            {
                Assert.Equal(exception.Message, error?.Message);
            }
        }
    }
}
