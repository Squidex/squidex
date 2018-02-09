// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Guard
{
    public class GuardContentTests
    {
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

        [Fact]
        public void CanCreate_should_throw_exception_if_data_is_null()
        {
            var command = new CreateContent();

            Assert.Throws<ValidationException>(() => GuardContent.CanCreate(command));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var command = new CreateContent { Data = new NamedContentData() };

            GuardContent.CanCreate(command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_data_is_null()
        {
            var command = new UpdateContent();

            Assert.Throws<ValidationException>(() => GuardContent.CanUpdate(command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            var command = new UpdateContent { Data = new NamedContentData() };

            GuardContent.CanUpdate(command);
        }

        [Fact]
        public void CanPatch_should_throw_exception_if_data_is_null()
        {
            var command = new PatchContent();

            Assert.Throws<ValidationException>(() => GuardContent.CanPatch(command));
        }

        [Fact]
        public void CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            var command = new PatchContent { Data = new NamedContentData() };

            GuardContent.CanPatch(command);
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_status_not_valid()
        {
            var command = new ChangeContentStatus { Status = (Status)10 };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(Status.Archived, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(Status.Archived, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_due_date_in_past()
        {
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(Status.Draft, command));
        }

        [Fact]
        public void CanChangeContentStatus_not_should_throw_exception_if_status_flow_valid()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            GuardContent.CanChangeContentStatus(Status.Draft, command);
        }

        [Fact]
        public void CanPatch_should_not_throw_exception()
        {
            var command = new DeleteContent();

            GuardContent.CanDelete(command);
        }
    }
}
