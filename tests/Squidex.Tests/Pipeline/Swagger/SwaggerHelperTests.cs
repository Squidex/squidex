// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using Squidex.Config;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;
using Xunit;

namespace Squidex.Tests.Pipeline.Swagger
{
    public class SwaggerHelperTests
    {
        private readonly IHttpContextAccessor contextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly string appName = "app";
        private readonly string host = "kraken";
        private readonly MyUrlsOptions myUrlsOptions = new MyUrlsOptions { BaseUrl = "www.test.com" };
        private readonly SwaggerOperation operation = new SwaggerOperation();

        [Fact]
        public void Should_load_docs()
        {
            var doc = SwaggerHelper.LoadDocs("security");
            Assert.StartsWith("Squidex", doc);
        }

        [Fact]
        public void Should_throw_exception_when_base_url_is_empty()
        {
            var testUrlOptions = new MyUrlsOptions();
            Assert.Throws<ConfigurationException>(() => testUrlOptions.BuildUrl("/api"));
        }

        [Fact]
        public void Should_create_swagger_document()
        {
            var swaggerDoc = CreateSwaggerDocument();

            Assert.NotNull(swaggerDoc.Tags);
            Assert.Contains("application/json", swaggerDoc.Consumes);
            Assert.Contains("application/json", swaggerDoc.Produces);
            Assert.NotNull(swaggerDoc.Info.ExtensionData["x-logo"]);
            Assert.Equal($"Squidex API for {appName} App", swaggerDoc.Info.Title);
            Assert.Equal("/api", swaggerDoc.BasePath);
            Assert.Equal(host, swaggerDoc.Host);
            Assert.NotEmpty(swaggerDoc.SecurityDefinitions);
        }

        [Fact]
        public void Should_create_OAuth_schema()
        {
            var oauthSchema = SwaggerHelper.CreateOAuthSchema(myUrlsOptions);

            Assert.Equal(myUrlsOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token"), oauthSchema.TokenUrl);
            Assert.Equal(SwaggerSecuritySchemeType.OAuth2, oauthSchema.Type);
            Assert.Equal(SwaggerOAuth2Flow.Application, oauthSchema.Flow);
            Assert.NotEmpty(oauthSchema.Scopes);
            Assert.Contains(myUrlsOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token"),
                oauthSchema.Description);
            Assert.DoesNotContain("<TOKEN_URL>", oauthSchema.Description);
        }

        [Fact]
        public async Task Should_get_error_dto_schema()
        {
            var swaggerDoc = CreateSwaggerDocument();

            var schemaGenerator = new SwaggerJsonSchemaGenerator(new SwaggerSettings());
            var schemaResolver = new SwaggerSchemaResolver(swaggerDoc, new SwaggerSettings());
            var errorDto = await schemaGenerator.GetErrorDtoSchemaAsync(schemaResolver);

            Assert.NotNull(errorDto);
        }

        [Fact]
        public void Should_add_query_parameter()
        {
            operation.AddQueryParameter("test", JsonObjectType.String, "Test parameter");
            Assert.Contains(operation.Parameters, p => p.Kind == SwaggerParameterKind.Query);
        }

        [Fact]
        public void Should_add_path_parameter()
        {
            operation.AddPathParameter("test", JsonObjectType.String, "Test parameter");
            Assert.Contains(operation.Parameters, p => p.Kind == SwaggerParameterKind.Path);
        }

        [Fact]
        public void Should_add_body_parameter()
        {
            operation.AddBodyParameter("test", null, "Test parameter");
            Assert.Contains(operation.Parameters, p => p.Kind == SwaggerParameterKind.Body);
        }

        [Fact]
        public void Should_add_response_parameter()
        {
            operation.AddResponse("200", "Test is ok");
            Assert.Contains(operation.Responses, r => r.Key == "200");
        }

        private SwaggerDocument CreateSwaggerDocument()
        {
            A.CallTo(() => contextAccessor.HttpContext.Request.Scheme).Returns("http");
            A.CallTo(() => contextAccessor.HttpContext.Request.Host).Returns(new HostString(host));
            return SwaggerHelper.CreateApiDocument(contextAccessor.HttpContext, myUrlsOptions, appName);
        }
    }
}
