// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class StatusTests
    {
        [Fact]
        public void Should_initialize_status_from_string()
        {
            var result = new Status2("Draft");

            Assert.Equal("Draft", result.Name);
            Assert.Equal("Draft", result.ToString());
        }

        [Fact]
        public void Should_provide_published_status()
        {
            var result = Status2.Published;

            Assert.Equal("Published", result.Name);
            Assert.Equal("Published", result.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var status_1_a = new Status2("Draft");
            var status_1_b = new Status2("Draft");

            var status2_a = new Status2("Published");

            Assert.Equal(status_1_a, status_1_b);
            Assert.Equal(status_1_a.GetHashCode(), status_1_b.GetHashCode());
            Assert.True(status_1_a.Equals((object)status_1_b));

            Assert.NotEqual(status_1_a, status2_a);
            Assert.NotEqual(status_1_a.GetHashCode(), status2_a.GetHashCode());
            Assert.False(status_1_a.Equals((object)status2_a));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var status = new Status2("Draft");

            var serialized = status.SerializeAndDeserialize();

            Assert.Equal(status, serialized);
        }
    }
}
