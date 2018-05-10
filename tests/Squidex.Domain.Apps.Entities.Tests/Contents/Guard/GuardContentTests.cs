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

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(false, Status.Archived, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(false, Status.Archived, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_due_date_in_past()
        {
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(false, Status.Draft, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_publishing_without_pending_changes()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            Assert.Throws<ValidationException>(() => GuardContent.CanChangeContentStatus(false, Status.Published, command));
        }

        [Fact]
        public void CanChangeContentStatus_should_not_throw_exception_if_publishing_with_pending_changes()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            GuardContent.CanChangeContentStatus(true, Status.Published, command);
        }

        [Fact]
        public void CanChangeContentStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            GuardContent.CanChangeContentStatus(false, Status.Draft, command);
        }

        [Fact]
        public void CanDiscardChanges_should_throw_exception_if_pending()
        {
            var command = new DiscardChanges();

            Assert.Throws<ValidationException>(() => GuardContent.CanDiscardChanges(false, command));
        }

        [Fact]
        public void CanDiscardChanges_should_not_throw_exception_if_pending()
        {
            var command = new DiscardChanges();

            GuardContent.CanDiscardChanges(true, command);
        }

        [Fact]
        public void CanPatch_should_not_throw_exception()
        {
            var command = new DeleteContent();

            GuardContent.CanDelete(command);
        }
    }
}
