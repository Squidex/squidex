// ==========================================================================
//  AppDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppDomainObjectTests : HandlerTestBase<AppDomainObject>
    {
        private readonly AppDomainObject sut;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";
        private readonly string planId = "premium";

        public AppDomainObjectTests()
        {
            sut = new AppDomainObject(AppId, 0);
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
            sut.Create(CreateCommand(new CreateApp { Name = AppName, Actor = User, AppId = AppId }));

            Assert.Equal(AppName, sut.Name);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }),
                    CreateEvent(new AppContributorAssigned { ContributorId = User.Identifier, Permission = AppContributorPermission.Owner }),
                    CreateEvent(new AppLanguageAdded { Language = Language.EN })
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

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageUpdated { Language = Language.DE, Fallback = new List<Language> { Language.EN } })
                );
        }

        private void CreateApp()
        {
            sut.Create(CreateCommand(new CreateApp { Name = AppName }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateClient()
        {
            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateLanguage(Language language)
        {
            sut.AddLanguage(CreateCommand(new AddLanguage { Language = language }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
