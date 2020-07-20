﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppSettingsSearchSourceTests
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly AppSettingsSearchSource sut;

        public AppSettingsSearchSourceTests()
        {
            sut = new AppSettingsSearchSource(urlGenerator);
        }

        [Fact]
        public async Task Should_empty_if_nothing_matching()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("xyz", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_always_return_contents_result_if_matching()
        {
            var ctx = ContextWithPermission();

            A.CallTo(() => urlGenerator.ContentsUI(appId))
                .Returns("contents-url");

            var result = await sut.SearchAsync("content", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Contents", SearchResultType.Content, "contents-url"));
        }

        [Fact]
        public async Task Should_always_return_dashboard_result_if_matching()
        {
            var ctx = ContextWithPermission();

            A.CallTo(() => urlGenerator.DashboardUI(appId))
                .Returns("dashboard-url");

            var result = await sut.SearchAsync("dashboard", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Dashboard", SearchResultType.Dashboard, "dashboard-url"));
        }

        [Fact]
        public async Task Should_always_return_languages_result_if_matching()
        {
            var ctx = ContextWithPermission(null);

            A.CallTo(() => urlGenerator.LanguagesUI(appId))
                .Returns("languages-url");

            var result = await sut.SearchAsync("languages", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Languages", SearchResultType.Setting, "languages-url"));
        }

        [Fact]
        public async Task Should_always_return_patterns_result_if_matching()
        {
            var ctx = ContextWithPermission();

            A.CallTo(() => urlGenerator.PatternsUI(appId))
                .Returns("patterns-url");

            var result = await sut.SearchAsync("patterns", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Patterns", SearchResultType.Setting, "patterns-url"));
        }

        [Fact]
        public async Task Should_always_return_schemas_result_if_matching()
        {
            var ctx = ContextWithPermission();

            A.CallTo(() => urlGenerator.SchemasUI(appId))
                .Returns("schemas-url");

            var result = await sut.SearchAsync("schemas", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Schemas", SearchResultType.Schema, "schemas-url"));
        }

        [Fact]
        public async Task Should_return_assets_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppAssetsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.AssetsUI(appId))
                .Returns("assets-url");

            var result = await sut.SearchAsync("assets", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Assets", SearchResultType.Asset, "assets-url"));
        }

        [Fact]
        public async Task Should_not_return_assets_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("assets", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_backups_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppBackupsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.BackupsUI(appId))
                .Returns("backups-url");

            var result = await sut.SearchAsync("backups", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Backups", SearchResultType.Setting, "backups-url"));
        }

        [Fact]
        public async Task Should_not_return_backups_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("backups", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_clients_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppClientsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.ClientsUI(appId))
                .Returns("clients-url");

            var result = await sut.SearchAsync("clients", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Clients", SearchResultType.Setting, "clients-url"));
        }

        [Fact]
        public async Task Should_not_return_clients_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("clients", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_contributors_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppContributorsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.ContributorsUI(appId))
                .Returns("contributors-url");

            var result = await sut.SearchAsync("contributors", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Contributors", SearchResultType.Setting, "contributors-url"));
        }

        [Fact]
        public async Task Should_not_contributors_clients_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("contributors", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_subscription_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppPlansRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.PlansUI(appId))
                .Returns("subscription-url");

            var result = await sut.SearchAsync("subscription", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Subscription", SearchResultType.Setting, "subscription-url"));
        }

        [Fact]
        public async Task Should_not_subscription_clients_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("subscription", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_roles_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppRolesRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.RolesUI(appId))
                .Returns("roles-url");

            var result = await sut.SearchAsync("roles", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Roles", SearchResultType.Setting, "roles-url"));
        }

        [Fact]
        public async Task Should_not_roles_clients_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("roles", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_rules_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppRulesRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.RulesUI(appId))
                .Returns("rules-url");

            var result = await sut.SearchAsync("rules", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Rules", SearchResultType.Rule, "rules-url"));
        }

        [Fact]
        public async Task Should_not_return_rules_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("assets", ctx);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_return_workflows_result_if_matching_and_permission_given()
        {
            var permission = Permissions.ForApp(Permissions.AppWorkflowsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            A.CallTo(() => urlGenerator.WorkflowsUI(appId))
                .Returns("workflows-url");

            var result = await sut.SearchAsync("workflows", ctx);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("Workflows", SearchResultType.Setting, "workflows-url"));
        }

        [Fact]
        public async Task Should_not_workflows_clients_result_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("workflows", ctx);

            Assert.Empty(result);
        }

        private Context ContextWithPermission(string? permission = null)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            if (permission == null)
            {
                permission = Permissions.ForApp(Permissions.AppCommon, appId.Name).Id;
            }

            claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));

            return new Context(claimsPrincipal, Mocks.App(appId));
        }
    }
}