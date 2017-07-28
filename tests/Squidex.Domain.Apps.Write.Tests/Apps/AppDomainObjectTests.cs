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

// ReSharper disable ConvertToConstant.Local

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
        public void Create_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateCommand(new CreateApp()));
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
                    CreateEvent(new AppContributorAssigned { ContributorId = User.Identifier, Permission = PermissionLevel.Owner }),
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
        public void ChangePlan_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.ChangePlan(CreateCommand(new ChangePlan()));
            });
        }

        [Fact]
        public void ChangePlan_should_throw_exception_if_plan_configured_from_other_user()
        {
            CreateApp();

            sut.ChangePlan(CreateCommand(new ChangePlan { PlanId = "other-plan", Actor = new RefToken("User", "other") }));

            Assert.Throws<ValidationException>(() =>
            {
                sut.ChangePlan(CreateCommand(new ChangePlan { PlanId = planId }));
            });
        }

        [Fact]
        public void ChangePlan_should_throw_exception_if_same_plan()
        {
            CreateApp();
            sut.ChangePlan(CreateCommand(new ChangePlan { PlanId = planId }));

            Assert.Throws<ValidationException>(() =>
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
        public void AssignContributor_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor()));
            });
        }

        [Fact]
        public void AssignContributor_should_throw_exception_if_single_owner_becomes_non_owner()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = User.Identifier, Permission = PermissionLevel.Editor }));
            });
        }

        [Fact]
        public void AssignContributor_should_throw_exception_if_user_already_contributor()
        {
            CreateApp();

            sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor }));

            Assert.Throws<ValidationException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor }));
            });
        }

        [Fact]
        public void AssignContributor_should_create_events()
        {
            CreateApp();

            sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Permission = PermissionLevel.Editor })
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
        public void RemoveContributor_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor()));
            });
        }

        [Fact]
        public void RemoveContributor_should_throw_exception_if_all_owners_removed()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = User.Identifier }));
            });
        }

        [Fact]
        public void RemoveContributor_should_throw_exception_if_contributor_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = "not-found" }));
            });
        }

        [Fact]
        public void RemoveContributor_should_create_events_and_remove_contributor()
        {
            CreateApp();

            sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor }));
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
        public void AttachClient_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient()));
            });

            Assert.Throws<ValidationException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient { Id = string.Empty }));
            });
        }

        [Fact]
        public void AttachClient_should_throw_exception_if_id_already_exists()
        {
            CreateApp();

            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }));

            Assert.Throws<ValidationException>(() =>
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
        public void RevokeClient_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RevokeClient(CreateCommand(new RevokeClient()));
            });

            Assert.Throws<ValidationException>(() =>
            {
                sut.RevokeClient(CreateCommand(new RevokeClient { Id = string.Empty }));
            });
        }

        [Fact]
        public void RevokeClient_should_throw_exception_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
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
        public void UpdateClient_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient()));
            });

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient { Id = string.Empty }));
            });
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient { Id = "not-found", Name = clientNewName }));
            });
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_client_has_same_reader_state()
        {
            CreateApp();
            CreateClient();

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient { Id = clientId, IsReader = false }));
            });
        }

        [Fact]
        public void UpdateClient_should_throw_exception_if_same_client_name()
        {
            CreateApp();
            CreateClient();

            sut.UpdateClient(CreateCommand(new UpdateClient { Id = clientId, Name = clientNewName }));

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateClient(CreateCommand(new UpdateClient { Id = clientId, Name = clientNewName }));
            });
        }

        [Fact]
        public void UpdateClient_should_create_events()
        {
            CreateApp();
            CreateClient();

            sut.UpdateClient(CreateCommand(new UpdateClient { Id = clientId, Name = clientNewName, IsReader = true }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRenamed { Id = clientId, Name = clientNewName }),
                    CreateEvent(new AppClientChanged { Id = clientId, IsReader = true })
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
        public void AddLanguage_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AddLanguage(CreateCommand(new AddLanguage()));
            });
        }

        [Fact]
        public void AddLanguage_should_throw_exception_if_language_already_exists()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AddLanguage(CreateCommand(new AddLanguage { Language = Language.EN }));
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
        public void RemoveLanguage_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage()));
            });
        }

        [Fact]
        public void RemoveLanguage_should_throw_exception_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void RemoveLanguage_should_throw_exception_if_master_language()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
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
        public void UpdateLanguage_should_throw_exception_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateLanguage(CreateCommand(new UpdateLanguage()));
            });
        }

        [Fact]
        public void UpdateLanguage_should_throw_exception_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.UpdateLanguage(CreateCommand(new UpdateLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void UpdateLanguage_should_throw_exception_if_master_language()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.UpdateLanguage(CreateCommand(new UpdateLanguage { Language = Language.EN, IsOptional = true }));
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
