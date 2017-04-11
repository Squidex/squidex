// ==========================================================================
//  AppDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Core.Apps;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Write.Apps.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Apps
{
    public class AppDomainObjectTests : HandlerTestBase<AppDomainObject>
    {
        private readonly AppDomainObject sut;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientSecret = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";

        public AppDomainObjectTests()
        {
            sut = new AppDomainObject(AppId, 0);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            CreateApp();

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateCommand(new CreateApp { Name = AppName }));
            });
        }

        [Fact]
        public void Create_should_throw_if_command_is_not_valid()
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
                    CreateEvent(new AppLanguageAdded { Language = Language.EN }),
                    CreateEvent(new AppMasterLanguageSet { Language  = Language.EN })
                );
        }

        [Fact]
        public void AssignContributor_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId }));
            });
        }

        [Fact]
        public void AssignContributor_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor()));
            });
        }

        [Fact]
        public void AssignContributor_should_throw_if_single_owner_becomes_non_owner()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AssignContributor(CreateCommand(new AssignContributor { ContributorId = User.Identifier, Permission = PermissionLevel.Editor }));
            });
        }

        [Fact]
        public void AssignContributor_should_throw_if_user_already_contributor()
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
        public void RemoveContributor_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = contributorId }));
            });
        }

        [Fact]
        public void RemoveContributor_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor()));
            });
        }

        [Fact]
        public void RemoveContributor_should_throw_if_all_owners_removed()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveContributor(CreateCommand(new RemoveContributor { ContributorId = User.Identifier }));
            });
        }

        [Fact]
        public void RemoveContributor_should_throw_if_contributor_not_found()
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
        public void AttachClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }), clientSecret);
            });
        }

        [Fact]
        public void AttachClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient()), clientSecret);
            });

            Assert.Throws<ValidationException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient { Id = string.Empty }), clientSecret);
            });
        }

        [Fact]
        public void AttachClient_should_throw_if_id_already_exists()
        {
            CreateApp();

            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }), clientSecret);

            Assert.Throws<ValidationException>(() =>
            {
                sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }), clientSecret);
            });
        }

        [Fact]
        public void AttachClient_should_create_events()
        {
            CreateApp();

            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }), clientSecret);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientAttached { Id = clientId, Secret = clientSecret })
                );
        }

        [Fact]
        public void RevokeClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RevokeClient(CreateCommand(new RevokeClient { Id = "not-found" }));
            });
        }

        [Fact]
        public void RevokeClient_should_throw_if_command_is_not_valid()
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
        public void RevokeClient_should_throw_if_client_not_found()
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
        public void RenameClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RenameClient(CreateCommand(new RenameClient { Id = "not-found", Name = clientNewName }));
            });
        }

        [Fact]
        public void RenameClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RenameClient(CreateCommand(new RenameClient()));
            });

            Assert.Throws<ValidationException>(() =>
            {
                sut.RenameClient(CreateCommand(new RenameClient { Id = string.Empty }));
            });
        }

        [Fact]
        public void RenameClient_should_throw_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.RenameClient(CreateCommand(new RenameClient { Id = "not-found", Name = clientNewName }));
            });
        }

        [Fact]
        public void RenameClient_should_throw_if_same_client_name()
        {
            CreateApp();
            CreateClient();

            sut.RenameClient(CreateCommand(new RenameClient { Id = clientId, Name = clientNewName }));

            Assert.Throws<ValidationException>(() =>
            {
                sut.RenameClient(CreateCommand(new RenameClient { Id = clientId, Name = clientNewName }));
            });
        }

        [Fact]
        public void RenameClient_should_create_events()
        {
            CreateApp();
            CreateClient();

            sut.RenameClient(CreateCommand(new RenameClient { Id = clientId, Name = clientNewName }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRenamed { Id = clientId, Name = clientNewName })
                );
        }

        [Fact]
        public void AddLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.AddLanguage(CreateCommand(new AddLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void AddLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.AddLanguage(CreateCommand(new AddLanguage()));
            });
        }

        [Fact]
        public void AddLanguage_should_throw_if_language_already_exists()
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
        public void RemoveLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.EN }));
            });
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage()));
            });
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_master_language()
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
            CreateLanguage();
            
            sut.RemoveLanguage(CreateCommand(new RemoveLanguage { Language = Language.DE }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageRemoved { Language = Language.DE })
                );
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.SetMasterLanguage(CreateCommand(new SetMasterLanguage { Language = Language.EN }));
            });
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.SetMasterLanguage(CreateCommand(new SetMasterLanguage()));
            });
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() =>
            {
                sut.SetMasterLanguage(CreateCommand(new SetMasterLanguage { Language = Language.DE }));
            });
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_already_master_language()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() =>
            {
                sut.SetMasterLanguage(CreateCommand(new SetMasterLanguage { Language = Language.EN }));
            });
        }

        [Fact]
        public void SetMasterLanguage_should_create_events()
        {
            CreateApp();
            CreateLanguage();

            sut.SetMasterLanguage(CreateCommand(new SetMasterLanguage { Language = Language.DE }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateEvent(new AppMasterLanguageSet { Language = Language.DE })
                );
        }

        private void CreateApp()
        {
            sut.Create(CreateCommand(new CreateApp { Name = AppName }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateClient()
        {
            sut.AttachClient(CreateCommand(new AttachClient { Id = clientId }), clientSecret);

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateLanguage()
        {
            sut.AddLanguage(CreateCommand(new AddLanguage { Language = Language.DE }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
