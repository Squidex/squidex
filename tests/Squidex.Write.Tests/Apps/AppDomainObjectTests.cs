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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Apps;
using Squidex.Write.Apps.Commands;
using Xunit;

namespace Squidex.Write.Tests.Apps
{
    public class AppDomainObjectTests
    {
        private const string TestName = "app";
        private readonly AppDomainObject sut = new AppDomainObject(Guid.NewGuid(), 0);
        private readonly string subjectId = Guid.NewGuid().ToString();
        private readonly string contributorId = Guid.NewGuid().ToString();

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            Assert.Throws<DomainException>(() => sut.Create(new CreateApp { Name = TestName }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_invalid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateApp()));
        }

        [Fact]
        public void Create_should_specify_and_owner()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            Assert.Equal(TestName, sut.Name);
            Assert.Equal(PermissionLevel.Owner, sut.Contributors[subjectId]);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppCreated { Name = TestName },
                        new AppContributorAssigned { ContributorId = subjectId, Permission = PermissionLevel.Owner }
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
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            Assert.Throws<ValidationException>(() => sut.AssignContributor(new AssignContributor { ContributorId = subjectId, Permission = PermissionLevel.Editor }));
        }

        [Fact]
        public void AssignContributor_should_create_events()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });
            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            Assert.Equal(PermissionLevel.Editor, sut.Contributors[contributorId]);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(2).ToArray()
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
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });

            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = subjectId }));
        }

        [Fact]
        public void RemoveContributor_should_throw_if_contributor_not_found()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });
            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });

            Assert.Throws<ValidationException>(() => sut.RemoveContributor(new RemoveContributor { ContributorId = "123" }));
        }

        [Fact]
        public void RemoveContributor_should_create_events_and_remove_contributor()
        {
            sut.Create(new CreateApp { Name = TestName, SubjectId = subjectId });
            sut.AssignContributor(new AssignContributor { ContributorId = contributorId, Permission = PermissionLevel.Editor });
            sut.RemoveContributor(new RemoveContributor { ContributorId = contributorId });

            Assert.False(sut.Contributors.ContainsKey(contributorId));

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(3).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new AppContributorRemoved { ContributorId = contributorId }
                    });
        }
    }
}
