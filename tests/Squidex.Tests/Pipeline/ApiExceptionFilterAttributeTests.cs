// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class ApiExceptionFilterAttributeTests
    {
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
        private readonly RouteData routeData = new RouteData();
        private readonly ApiExceptionFilterAttribute sut = new ApiExceptionFilterAttribute();
        private readonly ExceptionContext context;
        private ActionContext actionContext;

        public ApiExceptionFilterAttributeTests()
        {
            actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor.Object);
            context = new ExceptionContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void Domain_Object_Not_Found_Exception_should_be_caught()
        {
            context.Exception = new DomainObjectNotFoundException("id", typeof(IAppEntity));

            sut.OnException(context);

            Assert.Equal(new NotFoundResult().StatusCode, (context.Result as NotFoundResult).StatusCode);
        }

        [Fact]
        public void Domain_Object_Version_Exception_should_be_caught()
        {
            context.Exception = new DomainObjectVersionException("id", typeof(IAppEntity), 0, 1);

            sut.OnException(context);
            var exptectedResult = BuildErrorResult(412, new ErrorDto { Message = context.Exception.Message });

            Assert.Equal(exptectedResult.StatusCode, (context.Result as ObjectResult).StatusCode);
            Assert.StartsWith("Requested version", ((context.Result as ObjectResult).Value as ErrorDto).Message);
        }

        [Fact]
        public void Domain_Exception_should_be_caught()
        {
            context.Exception = new DomainException("Domain exception caught.");

            sut.OnException(context);
            var exptectedResult = BuildErrorResult(400, new ErrorDto { Message = context.Exception.Message });

            Assert.Equal(exptectedResult.StatusCode, (context.Result as ObjectResult).StatusCode);
            Assert.Equal("Domain exception caught.", ((context.Result as ObjectResult).Value as ErrorDto).Message);
        }

        [Fact]
        public void Domain_Forbidden_Exception_should_be_caught()
        {
            context.Exception = new DomainForbiddenException("Domain forbidden exception caught.");

            sut.OnException(context);
            var exptectedResult = BuildErrorResult(403, new ErrorDto { Message = context.Exception.Message });

            Assert.Equal(exptectedResult.StatusCode, (context.Result as ObjectResult).StatusCode);
            Assert.Equal("Domain forbidden exception caught.", ((context.Result as ObjectResult).Value as ErrorDto).Message);
        }

        [Fact]
        public void Validation_Exception_should_be_caught()
        {
            var errors = new ValidationError("Validation error 1", new string[] { "prop1" });
            context.Exception = new ValidationException("Validation exception caught.", errors);

            sut.OnException(context);
            var exptectedResult = BuildErrorResult(400, new ErrorDto { Message = context.Exception.Message });

            Assert.Equal(exptectedResult.StatusCode, (context.Result as ObjectResult).StatusCode);
            Assert.Equal("Validation exception caught: Validation error 1.", ((context.Result as ObjectResult).Value as ErrorDto).Message);
        }

        private ObjectResult BuildErrorResult(int code, ErrorDto error)
        {
            return new ObjectResult(error) { StatusCode = code };
        }
    }
}
