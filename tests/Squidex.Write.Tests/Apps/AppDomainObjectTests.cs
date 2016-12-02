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
        private readonly UserToken user = new UserToken("subject", Guid.NewGuid().ToString());
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientSecret = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";
        private readonly List<Language> languages = new List<Language> { Language.GetLanguage("de") };

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
            sut.Create(new CreateApp { Name = TestName, User = user });

            Assert.Equal(TestName, sut.Name);
            Assert.Equal(PermissionLevel.Owner, sut.Contributors[user.Identifier]);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppCreated { Name = TestName },
                        new AppContributorAssigned { ContributorId = user.Identifier, Permission = PermissionLevel.Owner },
                        new AppLanguagesConfigured { Languages= new List<Language> { Language.GetLanguage("en") } }
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
        public void AssignContributor_should_create_events()
        {
            CreateApp();

            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            Assert.Equal(PermissionLevel.Editor, sut.Contributors[contributorId]);

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

            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = "not-found" }));
        }

        [Fact]
        public void RemoveContributor_should_create_events_and_remove_contributor()
        {
            CreateApp();

            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });
            sut.RemoveContributor(new RemoveContributor { ContributorId = contributorId });

            Assert.False(sut.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppContributorRemoved { ContributorId = contributorId }
                    });
        }

        [Fact]
        public void ConfigureLanguages_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.ConfigureLanguages(new ConfigureLanguages { Languages = languages }));
        }

        [Fact]
        public void ConfigureLanguages_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.ConfigureLanguages(new ConfigureLanguages()));
            Assert.Throws<ValidationException>(() => sut.ConfigureLanguages(new ConfigureLanguages { Languages = new List<Language>() }));
        }

        [Fact]
        public void ConfigureLanguages_should_create_events()
        {
            CreateApp();

            sut.ConfigureLanguages(new ConfigureLanguages { Languages = languages });

            Assert.False(sut.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppLanguagesConfigured { Languages = languages }
                    });
        }

        [Fact]
        public void AttachClient_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AttachClient(new AttachClient { ClientId = clientId }, clientSecret));
        }

        [Fact]
        public void AttachClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient(), clientSecret));
            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient { ClientId = string.Empty }, clientSecret));
        }

        [Fact]
        public void AttachClient_should_throw_if_name_already_exists()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { ClientId = clientId }, clientSecret);

            Assert.Throws<ValidationException>(() => sut.AttachClient(new AttachClient { ClientId = clientId }, clientSecret));
        }

        [Fact]
        public void AttachClient_should_create_events()
        {
            var now = DateTime.Today;

            CreateApp();

            sut.AttachClient(new AttachClient { ClientId = clientId, Timestamp = now }, clientSecret);

            Assert.False(sut.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientAttached { ClientId = clientId, ClientSecret = clientSecret, ExpiresUtc = now.AddYears(1) }
                    });
        }

        [Fact]
        public void RevokeKey_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RevokeClient(new RevokeClient { ClientId = "not-found" }));
        }

        [Fact]
        public void RevokeClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RevokeClient(new RevokeClient()));
            Assert.Throws<ValidationException>(() => sut.RevokeClient(new RevokeClient { ClientId = string.Empty }));
        }

        [Fact]
        public void RevokeClient_should_throw_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RevokeClient(new RevokeClient { ClientId = "not-found" }));
        }

        [Fact]
        public void RevokeClient_should_create_events()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { ClientId = clientId }, clientSecret);
            sut.RevokeClient(new RevokeClient { ClientId = clientId });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientRevoked { ClientId = clientSecret }
                    });
        }

        [Fact]
        public void RenameKey_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RenameClient(new RenameClient { ClientId = "not-found", Name = clientNewName }));
        }

        [Fact]
        public void RenameClient_should_throw_if_command_is_not_valid()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient()));
            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient { ClientId = string.Empty }));
        }

        [Fact]
        public void RenameClient_should_throw_if_client_not_found()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RenameClient(new RenameClient { ClientId = "not-found", Name = clientNewName }));
        }

        [Fact]
        public void RenameClient_should_create_events()
        {
            CreateApp();

            sut.AttachClient(new AttachClient { ClientId = clientId }, clientSecret);
            sut.RenameClient(new RenameClient { ClientId = clientId, Name = clientNewName });

            Assert.Equal(clientNewName, sut.Clients[clientId].Name);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientRenamed { ClientId = clientId, Name = clientNewName }
                    });
        }

        private void CreateApp()
        {
            sut.Create(new CreateApp { Name = TestName, User = user });

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
