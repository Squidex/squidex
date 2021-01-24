// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Log;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class RequestExceptionMiddlewareTests
    {
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IActionResultExecutor<ObjectResult> resultWriter = A.Fake<IActionResultExecutor<ObjectResult>>();
        private readonly IHttpResponseFeature responseFeature = A.Fake<IHttpResponseFeature>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly RequestDelegate next;
        private bool isNextCalled;

        public RequestExceptionMiddlewareTests()
        {
            next = context =>
            {
                isNextCalled = true;

                return Task.CompletedTask;
            };

            httpContext.Features.Set(responseFeature);
        }

        [Fact]
        public async Task Should_write_test_error_if_valid_status_code()
        {
            httpContext.Request.QueryString = new QueryString("?error=412");

            var sut = new RequestExceptionMiddleware(next);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            Assert.False(isNextCalled);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._,
                    A<ObjectResult>.That.Matches(x => x.StatusCode == 412 && x.Value is ErrorDto)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_test_error_if_invalid_status_code()
        {
            httpContext.Request.QueryString = new QueryString("?error=hello");

            var sut = new RequestExceptionMiddleware(next);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            Assert.True(isNextCalled);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._, A<ObjectResult>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_test_error_if_invalid_error_status_code()
        {
            httpContext.Request.QueryString = new QueryString("?error=99");

            var sut = new RequestExceptionMiddleware(next);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            Assert.True(isNextCalled);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._, A<ObjectResult>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_handle_exception()
        {
            var failingNext = new RequestDelegate(context =>
            {
                throw new InvalidOperationException();
            });

            var sut = new RequestExceptionMiddleware(failingNext);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._,
                    A<ObjectResult>.That.Matches(x => x.StatusCode == 500 && x.Value is ErrorDto)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception()
        {
            var ex = new InvalidOperationException();

            var failingNext = new RequestDelegate(context =>
            {
                throw ex;
            });

            var sut = new RequestExceptionMiddleware(failingNext);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            A.CallTo(() => log.Log(SemanticLogLevel.Error, ex, A<LogFormatter>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_handle_exception_if_response_body_written()
        {
            A.CallTo(() => responseFeature.HasStarted)
                .Returns(true);

            var failingNext = new RequestDelegate(context =>
            {
                throw new InvalidOperationException();
            });

            var sut = new RequestExceptionMiddleware(failingNext);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._, A<ObjectResult>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_handle_error_status_code()
        {
            var failingNext = new RequestDelegate(context =>
            {
                context.Response.StatusCode = 412;

                return Task.CompletedTask;
            });

            var sut = new RequestExceptionMiddleware(failingNext);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._,
                    A<ObjectResult>.That.Matches(x => x.StatusCode == 412 && x.Value is ErrorDto)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_handle_error_status_code_if_response_body_written()
        {
            A.CallTo(() => responseFeature.HasStarted)
                .Returns(true);

            var failingNext = new RequestDelegate(context =>
            {
                context.Response.StatusCode = 412;

                return Task.CompletedTask;
            });

            var sut = new RequestExceptionMiddleware(failingNext);

            await sut.InvokeAsync(httpContext, resultWriter, log);

            A.CallTo(() => resultWriter.ExecuteAsync(A<ActionContext>._, A<ObjectResult>._))
                .MustNotHaveHappened();
        }
    }
}
