//// ==========================================================================
////  Squidex Headless CMS
//// ==========================================================================
////  Copyright (c) Squidex UG (haftungsbeschraenkt)
////  All rights reserved. Licensed under the MIT license.
//// ==========================================================================

//using FakeItEasy;
//using Microsoft.AspNetCore.Http;
//using Squidex.Domain.Apps.Entities;
//using Squidex.Domain.Apps.Entities.Contents.Commands;
//using Squidex.Domain.Apps.Entities.Teams.Commands;
//using Squidex.Domain.Apps.Entities.TestHelpers;
//using Squidex.Infrastructure;
//using Squidex.Infrastructure.Commands;
//using Squidex.Web.Pipeline;
//using Xunit;

//namespace Squidex.Web.CommandMiddlewares
//{
//    public class EnrichWithTeamIdCommandMiddlewareTests
//    {
//        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
//        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
//        private readonly NamedId<DomainId> teamId = NamedId.Of(DomainId.NewGuid(), "my-team");
//        private readonly HttpContext httpContext = new DefaultHttpContext();
//        private readonly EnrichWithTeamIdCommandMiddleware sut;

//        public EnrichWithTeamIdCommandMiddlewareTests()
//        {
//            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));

//            A.CallTo(() => httpContextAccessor.HttpContext)
//                .Returns(httpContext);

//            sut = new EnrichWithTeamIdCommandMiddleware(httpContextAccessor);
//        }

//        [Fact]
//        public async Task Should_throw_exception_if_team_not_found()
//        {
//            await Assert.ThrowsAsync<InvalidOperationException>(() => HandleAsync(new CreateContent()));
//        }

//        [Fact]
//        public async Task Should_assign_team_id_and_name_to_app_command()
//        {
//            httpContext.Features.Set<ITeamFeature>(new TeamFeature(Mocks.Team(appId, teamId)));

//            var context = await HandleAsync(new CreateContent());

//            Assert.Equal(teamId, ((ITeamCommand)context.Command).TeamId);
//        }

//        [Fact]
//        public async Task Should_assign_team_id_from_id()
//        {
//            httpContext.Features.Set<ITeamFeature>(new TeamFeature(Mocks.Team(appId, teamId)));

//            var context = await HandleAsync(new UpdateTeam());

//            Assert.Equal(teamId, ((ITeamCommand)context.Command).TeamId);
//        }

//        [Fact]
//        public async Task Should_not_override_team_id()
//        {
//            httpContext.Features.Set<ITeamFeature>(new TeamFeature(Mocks.Team(appId, teamId)));

//            var customId = DomainId.NewGuid();

//            var context = await HandleAsync(new CreateTeam { TeamId = customId });

//            Assert.Equal(customId, ((CreateTeam)context.Command).TeamId);
//        }

//        [Fact]
//        public async Task Should_not_override_team_id_and_name()
//        {
//            httpContext.Features.Set<ITeamFeature>(new TeamFeature(Mocks.Team(appId, teamId)));

//            var customId = NamedId.Of(DomainId.NewGuid(), "other-app");

//            var context = await HandleAsync(new CreateContent { TeamId = customId });

//            Assert.Equal(customId, ((ITeamCommand)context.Command).TeamId);
//        }

//        private async Task<CommandContext> HandleAsync(IAppCommand command)
//        {
//            command.AppId = appId;

//            var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

//            await sut.HandleAsync(commandContext, default);

//            return commandContext;
//        }
//    }
//}
