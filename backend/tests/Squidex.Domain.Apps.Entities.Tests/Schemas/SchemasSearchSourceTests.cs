﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemasSearchSourceTests : IClassFixture<TranslationsFixture>
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly SchemasSearchSource sut;

        public SchemasSearchSourceTests()
        {
            sut = new SchemasSearchSource(appProvider, urlGenerator);
        }

        [Fact]
        public async Task Should_return_result_to_schema_only_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var schema1 = CreateSchema("schemaA1", false);

            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id))
                .Returns(new List<ISchemaEntity> { schema1 });

            A.CallTo(() => urlGenerator.SchemaUI(appId, schema1.NamedId()))
                .Returns("schemaA1-url");

            var result = await sut.SearchAsync("schema", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url"));
        }

        [Fact]
        public async Task Should_return_result_to_schema_and_contents_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppContentsReadOwn, appId.Name, "schemaA2");

            var ctx = ContextWithPermission(permission.Id);

            var schema1 = CreateSchema("schemaA1", false);
            var schema2 = CreateSchema("schemaA2", false);
            var schema3 = CreateSchema("schemaB2", false);

            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id))
                .Returns(new List<ISchemaEntity> { schema1, schema2, schema3 });

            A.CallTo(() => urlGenerator.SchemaUI(appId, schema1.NamedId()))
                .Returns("schemaA1-url");

            A.CallTo(() => urlGenerator.SchemaUI(appId, schema2.NamedId()))
                .Returns("schemaA2-url");

            A.CallTo(() => urlGenerator.ContentsUI(appId, schema2.NamedId()))
                .Returns("schemaA2-contents-url");

            var result = await sut.SearchAsync("schemaA", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                    .Add("schemaA2 Schema", SearchResultType.Schema, "schemaA2-url")
                    .Add("schemaA2 Contents", SearchResultType.Content, "schemaA2-contents-url", "schemaA2"));
        }

        [Fact]
        public async Task Should_return_result_to_schema_and_contents_if_schema_is_singleton()
        {
            var permission = Permissions.ForApp(Permissions.AppContentsReadOwn, appId.Name, "schemaA1");

            var ctx = ContextWithPermission(permission.Id);

            var schema1 = CreateSchema("schemaA1", true);

            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id))
                .Returns(new List<ISchemaEntity> { schema1 });

            A.CallTo(() => urlGenerator.SchemaUI(appId, schema1.NamedId()))
                .Returns("schemaA1-url");

            A.CallTo(() => urlGenerator.ContentUI(appId, schema1.NamedId(), schema1.Id))
                .Returns("schemaA1-content-url");

            var result = await sut.SearchAsync("schemaA", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                    .Add("schemaA1 Content", SearchResultType.Content, "schemaA1-content-url", "schemaA1"));
        }

        private ISchemaEntity CreateSchema(string name, bool isSingleton)
        {
            return Mocks.Schema(appId, NamedId.Of(DomainId.NewGuid(), name), new Schema(name, null, isSingleton));
        }

        private Context ContextWithPermission(string? permission = null)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            if (permission != null)
            {
                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }

            return new Context(claimsPrincipal, Mocks.App(appId));
        }
    }
}
