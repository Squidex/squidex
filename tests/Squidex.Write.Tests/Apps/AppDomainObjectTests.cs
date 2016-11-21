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
using Squidex.Write.Apps;
using Squidex.Write.Apps.Commands;
using Xunit;

namespace Squidex.Write.Tests.Apps
{
    public class AppDomainObjectTests
    {
        private const string TestName = "app";
        private readonly AppDomainObject sut;
        private readonly string subjectId = Guid.NewGuid().ToString();
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientKey = Guid.NewGuid().ToString();
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
        public void Create_should_throw_if_command_is_invalid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateApp()));
        }

        [Fact]
        public void Create_should_specify_name_and_owner()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            Assert.Equal(TestName, sut.Name);
            Assert.Equal(PermissionLevel.Owner, sut.Contributors[subjectId]);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppCreated { Name = TestName },
                        new AppContributorAssigned { ContributorId = subjectId, Permission = PermissionLevel.Owner },
                        new AppLanguagesConfigured { Languages= new List<Language> { Language.GetLanguage("de") } }
                    });
        }

        [Fact]
        public void AssignContributor_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AssignContributor(new AssignContributor { ContributorId = contributorId }));
        }

        [Fact]
        public void AssignContributor_should_throw_if_single_owner_becomes_non_owner()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.AssignContributor(new AssignContributor { ContributorId = subjectId, Permission = PermissionLevel.Editor }));
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
        public void RemoveContributor_should_throw_if_all_owners_removed()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = subjectId }));
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
        public void ConfigureLanguages_should_throw_if_languages_are_null_or_empty()
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
        public void CreateClientKey_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.CreateClientKey(new CreateClientKey { ClientKey = clientKey }));
        }

        [Fact]
        public void CreateClientKey_should_throw_if_client_key_is_null_or_empty()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.CreateClientKey(new CreateClientKey()));
            Assert.Throws<ValidationException>(() => sut.CreateClientKey(new CreateClientKey { ClientKey = string.Empty }));
        }

        [Fact]
        public void CreateClientKey_should_create_events()
        {
            var now = DateTime.Today;

            CreateApp();

            sut.CreateClientKey(new CreateClientKey { ClientKey = clientKey, Timestamp = now });

            Assert.False(sut.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientKeyCreated { ClientKey = clientKey, ExpiresUtc = now.AddYears(1) }
                    });
        }

        [Fact]
        public void RevokeClientKey_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.RevokeClientKey(new RevokeClientKey { ClientKey = "not-found" }));
        }

        [Fact]
        public void RevokeClientKey_should_throw_if_client_key_is_null_or_empty()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RevokeClientKey(new RevokeClientKey()));
            Assert.Throws<ValidationException>(() => sut.RevokeClientKey(new RevokeClientKey { ClientKey = string.Empty }));
        }

        [Fact]
        public void RevokeClientKey_should_throw_if_key_not_found()
        {
            CreateApp();

            Assert.Throws<ValidationException>(() => sut.RevokeClientKey(new RevokeClientKey { ClientKey = "not-found" }));
        }

        [Fact]
        public void RevokeClientKey_should_create_events()
        {
            CreateApp();

            sut.CreateClientKey(new CreateClientKey { ClientKey = clientKey });
            sut.RevokeClientKey(new RevokeClientKey { ClientKey = clientKey });

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppClientKeyRevoked { ClientKey = clientKey }
                    });
        }

        private void CreateApp()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
