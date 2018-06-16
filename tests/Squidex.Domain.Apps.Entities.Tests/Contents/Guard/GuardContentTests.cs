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
using Squidex.Domain.Apps.Entities.TestHelpers;
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

            ValidationAssert.Throws(() => GuardContent.CanCreate(command),
                new ValidationError("Data is required.", "Data"));
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

            ValidationAssert.Throws(() => GuardContent.CanUpdate(command),
                new ValidationError("Data is required.", "Data"));
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

            ValidationAssert.Throws(() => GuardContent.CanPatch(command),
                new ValidationError("Data is required.", "Data"));
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

            ValidationAssert.Throws(() => GuardContent.CanChangeContentStatus(false, Status.Archived, command),
                new ValidationError("Status is not valid.", "Status"));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            ValidationAssert.Throws(() => GuardContent.CanChangeContentStatus(false, Status.Archived, command),
                new ValidationError("Cannot change status from Archived to Published.", "Status"));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_due_date_in_past()
        {
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast };

            ValidationAssert.Throws(() => GuardContent.CanChangeContentStatus(false, Status.Draft, command),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public void CanChangeContentStatus_should_throw_exception_if_publishing_without_pending_changes()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            ValidationAssert.Throws(() => GuardContent.CanChangeContentStatus(false, Status.Published, command),
                new ValidationError("Content has no changes to publish.", "Status"));
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

            Assert.Throws<DomainException>(() => GuardContent.CanDiscardChanges(false, command));
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
