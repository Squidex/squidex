// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Entities
{
    public class AppProviderTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IAppsIndex indexForApps = A.Fake<IAppsIndex>();
        private readonly IRulesIndex indexForRules = A.Fake<IRulesIndex>();
        private readonly ISchemasIndex indexForSchemas = A.Fake<ISchemasIndex>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly IAppEntity app;
        private readonly AppProvider sut;

        public AppProviderTests()
        {
            ct = cts.Token;

            app = Mocks.App(appId);

            sut = new AppProvider(indexForApps, indexForRules, indexForSchemas, new AsyncLocalCache());
        }

        [Fact]
        public async Task Should_get_app_with_schema_from_index()
        {
            var schema = Mocks.Schema(app.NamedId(), schemaId);

            A.CallTo(() => indexForApps.GetAppAsync(app.Id, false, ct))
                .Returns(app);

            A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schema.Id, false, ct))
                .Returns(schema);

            var result = await sut.GetAppWithSchemaAsync(app.Id, schemaId.Id, false, ct);

            Assert.Equal(schema, result.Item2);
        }

        [Fact]
        public async Task Should_get_apps_from_index()
        {
            var permissions = new PermissionSet("*");

            A.CallTo(() => indexForApps.GetAppsForUserAsync("user1", permissions, ct))
                .Returns(new List<IAppEntity> { app });

            var result = await sut.GetUserAppsAsync("user1", permissions, ct);

            Assert.Equal(app, result.Single());
        }

        [Fact]
        public async Task Should_get_app_from_index()
        {
            A.CallTo(() => indexForApps.GetAppAsync(app.Id, false, ct))
                .Returns(app);

            var result = await sut.GetAppAsync(app.Id, false, ct);

            Assert.Equal(app, result);
        }

        [Fact]
        public async Task Should_get_app_by_name_from_index()
        {
            A.CallTo(() => indexForApps.GetAppAsync(app.Name, false, ct))
                .Returns(app);

            var result = await sut.GetAppAsync(app.Name, false, ct);

            Assert.Equal(app, result);
        }

        [Fact]
        public async Task Should_get_schema_from_index()
        {
            var schema = Mocks.Schema(app.NamedId(), schemaId);

            A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schema.Id, false, ct))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(app.Id, schema.Id, false, ct);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_get_schema_by_name_from_index()
        {
            var schema = Mocks.Schema(app.NamedId(), schemaId);

            A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schemaId.Name, false, ct))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(app.Id, schemaId.Name, false, ct);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_get_schemas_from_index()
        {
            var schema = Mocks.Schema(app.NamedId(), schemaId);

            A.CallTo(() => indexForSchemas.GetSchemasAsync(app.Id, ct))
                .Returns(new List<ISchemaEntity> { schema });

            var result = await sut.GetSchemasAsync(app.Id, ct);

            Assert.Equal(schema, result.Single());
        }

        [Fact]
        public async Task Should_get_rules_from_index()
        {
            var rule = new RuleEntity();

            A.CallTo(() => indexForRules.GetRulesAsync(app.Id, ct))
                .Returns(new List<IRuleEntity> { rule });

            var result = await sut.GetRulesAsync(app.Id, ct);

            Assert.Equal(rule, result.Single());
        }

        [Fact]
        public async Task Should_get_rule_from_index()
        {
            var rule = new RuleEntity { Id = DomainId.NewGuid() };

            A.CallTo(() => indexForRules.GetRulesAsync(app.Id, ct))
                .Returns(new List<IRuleEntity> { rule });

            var result = await sut.GetRuleAsync(app.Id, rule.Id, ct);

            Assert.Equal(rule, result);
        }
    }
}
