// ==========================================================================
//  AppDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using Squidex.Core.Apps;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Apps.Commands;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Apps
{
    public class AppDomainObjectTests
    {
        private const string TestName = "app";
        private readonly AppDomainObject sut;
        private readonly RefToken user = new RefToken("subject", Guid.NewGuid().ToString());
        private readonly DateTime expiresUtc = DateTime.UtcNow.AddYears(1);
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientSecret = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";

        public AppDomainObjectTests()
        {
            sut = new AppDomainObject(Guid.NewGuid(), 0);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            CreateApp();

            Assert.Throws<DomainException>(() => sut.Create(new CreateApp { Name = TestName }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateApp()));
        }

        [Fact]
        public void Create_should_specify_name_and_owner()
        {
            sut.Create(new CreateApp { Name = TestName, Actor = user });

            Assert.Equal(TestName, sut.Name);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppCreated { Name = TestName },
                        new AppContributorAssigned { ContributorId = user.Identifier, Permission = PermissionLevel.Owner },
                        new AppLanguageAdded { Language = Language.EN },
                        new AppMasterLanguageSet { Language  = Language.EN }
                    });
        }

        [Fact]
        public void AssignContributor_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AssignContributor(new AssignContributor { ContributorId = contributorId }));
        }

        [Fact]
        public void AssignContributor_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.AssignContributor(new AssignContributor()));
        }

        [Fact]
        public void AssignContributor_should_throw_if_single_owner_becomes_non_owner()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AssignContributor(new AssignContributor { ContributorId = user.Identifier, Permission = PermissionLevel.Editor }));
        }

        [Fact]
        public void AssignContributor_should_throw_if_user_already_contributor()
        {
            CreateApp();
            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            Assert.Throws<ValidationException>(() => sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor }));
        }

        [Fact]
        public void AssignContributor_should_create_events()
        {
            CreateApp();

            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppContributorAssigned { ContributorId = contributorId, Permission = PermissionLevel.Editor }
                    });
        }

        [Fact]
        public void RemoveContributor_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = contributorId }));
        }

        [Fact]
        public void RemoveContributor_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor()));
        }

        [Fact]
        public void RemoveContributor_should_throw_if_all_owners_removed()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = user.Identifier }));
        }

        [Fact]
        public void RemoveContributor_should_throw_if_contributor_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = "not-found" }));
        }

        [Fact]
        public void RemoveContributor_should_create_events_and_remove_contributor()
        {
            CreateApp();

            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });
            sut.RemoveContributor(new RemoveContributor { ContributorId = contributorId });
            
            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppContributorRemoved { ContributorId = contributorId }
                    });
        }

        [Fact]
        public void AttachClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc));
        }

        [Fact]
        public void AttachClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient(), clientSecret, expiresUtc));
            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient { Id = string.Empty }, clientSecret, expiresUtc));
        }

        [Fact]
        public void AttachClient_should_throw_if_id_already_exists()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc);

            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc));
        }

        [Fact]
        public void AttachClient_should_create_events()
        {
            var now = DateTime.Today;

            CreateApp();

            sut.AttachClient(new AttachClient { Id = clientId, Timestamp = now }, clientSecret, expiresUtc);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientAttached { Id = clientId, Secret = clientSecret, ExpiresUtc = expiresUtc }
                    });
        }

        [Fact]
        public void RevokeClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RevokeClient(new RevokeClient { Id = "not-found" }));
        }

        [Fact]
        public void RevokeClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RevokeClient(new RevokeClient()));
            Assert.Throws<ValidationException>(() => sut.RevokeClient(new RevokeClient { Id = string.Empty }));
        }

        [Fact]
        public void RevokeClient_should_throw_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.RevokeClient(new RevokeClient { Id = "not-found" }));
        }

        [Fact]
        public void RevokeClient_should_create_events()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc);
            sut.RevokeClient(new RevokeClient { Id = clientId });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientRevoked { Id = clientSecret }
                    });
        }

        [Fact]
        public void RenameClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RenameClient(new RenameClient { Id = "not-found", Name = clientNewName }));
        }

        [Fact]
        public void RenameClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient()));
            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient { Id = string.Empty }));
        }

        [Fact]
        public void RenameClient_should_throw_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.RenameClient(new RenameClient { Id = "not-found", Name = clientNewName }));
        }

        [Fact]
        public void RenameClient_should_throw_if_same_client_name()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc);
            sut.RenameClient(new RenameClient { Id = clientId, Name = clientNewName });

            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient { Id = clientId, Name = clientNewName }));
        }

        [Fact]
        public void RenameClient_should_create_events()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { Id = clientId }, clientSecret, expiresUtc);
            sut.RenameClient(new RenameClient { Id = clientId, Name = clientNewName });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientRenamed { Id = clientId, Name = clientNewName }
                    });
        }

        [Fact]
        public void AddLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AddLanguage(new AddLanguage { Language = Language.DE }));
        }

        [Fact]
        public void AddLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AddLanguage(new AddLanguage()));
        }

        [Fact]
        public void AddLanguage_should_throw_if_language_already_exists()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AddLanguage(new AddLanguage { Language = Language.EN }));
        }

        [Fact]
        public void AddLanguage_should_create_events()
        {
            CreateApp();

            sut.AddLanguage(new AddLanguage { Language = Language.DE });

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppLanguageAdded { Language = Language.DE }
                    });
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RemoveLanguage(new RemoveLanguage { Language = Language.EN }));
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RemoveLanguage(new RemoveLanguage()));
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.RemoveLanguage(new RemoveLanguage { Language = Language.DE }));
        }

        [Fact]
        public void RemoveLanguage_should_throw_if_master_language()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RemoveLanguage(new RemoveLanguage { Language = Language.EN }));
        }

        [Fact]
        public void RemoveLanguage_should_create_events()
        {
            CreateApp();

            sut.AddLanguage(new AddLanguage { Language = Language.DE });
            sut.RemoveLanguage(new RemoveLanguage { Language = Language.DE });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppLanguageRemoved { Language = Language.DE }
                    });
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.SetMasterLanguage(new SetMasterLanguage { Language = Language.EN }));
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.SetMasterLanguage(new SetMasterLanguage()));
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_language_not_found()
        {
            CreateApp();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.SetMasterLanguage(new SetMasterLanguage { Language = Language.DE }));
        }

        [Fact]
        public void SetMasterLanguage_should_throw_if_already_master_language()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.SetMasterLanguage(new SetMasterLanguage { Language = Language.EN }));
        }

        [Fact]
        public void SetMasterLanguage_should_create_events()
        {
            CreateApp();

            sut.AddLanguage(new AddLanguage { Language = Language.DE });
            sut.SetMasterLanguage(new SetMasterLanguage { Language = Language.DE });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppMasterLanguageSet { Language = Language.DE }
                    });
        }

        private void CreateApp()
        {
            sut.Create(new CreateApp { Name = TestName, Actor = user });

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
