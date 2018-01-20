// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppDomainObjectTests : HandlerTestBase<AppDomainObject>
    {
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";
        private readonly string planId = "premium";
        private readonly Guid patternId = Guid.NewGuid();
        private readonly AppDomainObject sut = new AppDomainObject(new InitialPatterns());

        protected override Guid Id
        {
            get { return AppId; }
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            CreateApp();

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateCommand(new CreateApp { Name = AppName }));
            });
        }

        [Fact]
        public void Create_should_specify_name_and_owner()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var initialPatterns = new InitialPatterns
            {
                { id1, new AppPattern("Number", "[0-9]") },
                { id2, new AppPattern("Numbers", "[0-9]*") }
            };

            var app = new AppDomainObject(initialPatterns);

            app.Create(CreateCommand(new CreateApp { Name = AppName, Actor = User, AppId = AppId }));

            Assert.Equal(AppName, app.Snapshot.Name);

            app.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }),
                    CreateEvent(new AppContributorAssigned { ContributorId = User.Identifier, Permission = AppContributorPermission.Owner }),
                    CreateEvent(new AppLanguageAdded { Language = Language.EN }),
                    CreateEvent(new AppPatternAdded { PatternId = id1, Name = "Number", Pattern = "[0-9]" }),
                    CreateEvent(new AppPatternAdded { PatternId = id2, Name = "Numbers", Pattern = "[0-9]*" })
                );
        }

        [Fact]
        public void ChangePlan_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.ChangePlan(CreateCommand(new ChangePlan { PlanId = planId }));
            });
        }

        [Fact]
        public void ChangePlan_should_create_events()
        {
            CreateApp();

            sut.ChangePlan(CreateCommand(new ChangePlan { PlanId = planId }));

            Assert.Equal(planId, sut.Snapshot.Plan.PlanId);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanChanged { PlanId = planId })
                );
        }

        [Fact]
        public void AssignContributor_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId }));
            });
        }

        [Fact]
        public void AssignContributor_should_create_events()
        {
            CreateApp();

            sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = AppContributorPermission.Editor }));

            Assert.Equal(AppContributorPermission.Editor, sut.Snapshot.Contributors[contributorId]);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Permission = AppContributorPermission.Editor })
                );
        }

        [Fact]
        public void RemoveContributor_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = contributorId }));
            });
        }

        [Fact]
        public void RemoveContributor_should_create_events_and_remove_contributor()
        {
            CreateApp();

            sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = AppContributorPermission.Editor }));
            sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = contributorId }));

            Assert.False(sut.Snapshot.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Skip(1)
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorRemoved { ContributorId = contributorId })
                );
        }

        [Fact]
        public void AttachClient_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }));
            });
        }

        [Fact]
        public void AttachClient_should_create_events()
        {
            var command = new AttachClient { Id = clientId };

            CreateApp();

            sut.AttachClient(CreateCommand(command));

            Assert.True(sut.Snapshot.Clients.ContainsKey(clientId));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientAttached { Id = clientId, Secret = command.Secret })
                );
        }

        [Fact]
        public void RevokeClient_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RevokeClient(CreateCommand(new RevokeClient { Id = "not-found" }));
            });
        }

        [Fact]
        public void RevokeClient_should_create_events()
        {
            CreateApp();
            CreateClient();

            sut.RevokeClient(CreateCommand(new RevokeClient { Id = clientId }));

            Assert.False(sut.Snapshot.Clients.ContainsKey(clientId));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRevoked { Id = clientId })
                );
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient { Id = "not-found", Name = clientNewName }));
            });
        }

        [Fact]
        public void UpdateClient_should_create_events()
        {
            CreateApp();
            CreateClient();

            sut.UpdateClient(CreateCommand(new UpdateClient { Id = clientId, Name = clientNewName, Permission = AppClientPermission.Developer }));

            Assert.Equal(clientNewName, sut.Snapshot.Clients[clientId].Name);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRenamed { Id = clientId, Name = clientNewName }),
                    CreateEvent(new AppClientUpdated { Id = clientId, Permission = AppClientPermission.Developer })
                );
        }

        [Fact]
        public void AddLanguage_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AddLanguage(CreateCommand(new AddLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void AddLanguage_should_create_events()
        {
            CreateApp();

            sut.AddLanguage(CreateCommand(new AddLanguage { Language = Language.DE }));

            Assert.True(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageAdded { Language = Language.DE })
                );
        }

        [Fact]
        public void RemoveLanguage_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.EN }));
            });
        }

        [Fact]
        public void RemoveLanguage_should_create_events()
        {
            CreateApp();
            CreateLanguage(Language.DE);

            sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.DE }));

            Assert.False(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageRemoved { Language = Language.DE })
                );
        }

        [Fact]
        public void UpdateLanguage_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.UpdateLanguage(CreateCommand(new UpdateLanguage { Language = Language.EN }));
            });
        }

        [Fact]
        public void UpdateLanguage_should_create_events()
        {
            CreateApp();
            CreateLanguage(Language.DE);

            sut.UpdateLanguage(CreateCommand(new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.EN } }));

            Assert.True(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageUpdated { Language = Language.DE, Fallback = new List<Language> { Language.EN } })
                );
        }

        [Fact]
        public void AddPattern_should_throw_exception_if_app_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AddPattern(CreateCommand(new AddPattern { PatternId = patternId, Name = "Any", Pattern = ".*" })));
        }

        [Fact]
        public void AddPattern_should_create_events()
        {
            CreateApp();

            sut.AddPattern(CreateCommand(new AddPattern { PatternId = patternId, Name = "Any", Pattern = ".*", Message = "Msg" }));

            Assert.Single(sut.Snapshot.Patterns);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternAdded { PatternId = patternId, Name = "Any", Pattern = ".*", Message = "Msg" })
                );
        }

        [Fact]
        public void DeletePattern_should_throw_exception_if_app_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.DeletePattern(CreateCommand(new DeletePattern
                {
                    PatternId = Guid.NewGuid()
                }));
            });
        }

        [Fact]
        public void DeletePattern_should_create_events()
        {
            CreateApp();
            CreatePattern();

            sut.DeletePattern(CreateCommand(new DeletePattern { PatternId = patternId }));

            Assert.Empty(sut.Snapshot.Patterns);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternDeleted { PatternId = patternId })
                );
        }

        [Fact]
        public void UpdatePattern_should_throw_exception_if_app_not_created()
        {
            Assert.Throws<DomainException>(() => sut.UpdatePattern(CreateCommand(new UpdatePattern { PatternId = patternId, Name = "Any", Pattern = ".*" })));
        }

        [Fact]
        public void UpdatePattern_should_create_events()
        {
            CreateApp();
            CreatePattern();

            sut.UpdatePattern(CreateCommand(new UpdatePattern { PatternId = patternId, Name = "Any", Pattern = ".*", Message = "Msg" }));

            Assert.Single(sut.Snapshot.Patterns);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternUpdated { PatternId = patternId, Name = "Any", Pattern = ".*", Message = "Msg" })
                );
        }

        private void CreatePattern()
        {
            sut.AddPattern(CreateCommand(new AddPattern { PatternId = patternId, Name = "Name", Pattern = ".*" }));
            sut.ClearUncommittedEvents();
        }

        private void CreateApp()
        {
            sut.Create(CreateCommand(new CreateApp { Name = AppName }));
            sut.ClearUncommittedEvents();
        }

        private void CreateClient()
        {
            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }));
            sut.ClearUncommittedEvents();
        }

        private void CreateLanguage(Language language)
        {
            sut.AddLanguage(CreateCommand(new AddLanguage { Language = language }));
            sut.ClearUncommittedEvents();
        }
    }
}
