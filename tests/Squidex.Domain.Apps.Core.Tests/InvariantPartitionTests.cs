// ==========================================================================
//  InvariantPartitionTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Core
{
    public sealed class InvariantPartitionTests
    {
        [Fact]
        public void Should_provide_single_value()
        {
            var sut = InvariantPartitioning.Instance;

            Assert.Equal(1, sut.Count);

            Assert.Equal(sut.Master, ((IEnumerable<IFieldPartitionItem>)sut).SingleOrDefault());
            Assert.Equal(sut.Master, ((IEnumerable)sut).OfType<IFieldPartitionItem>().SingleOrDefault());
        }

        [Fact]
        public void Should_provide_master()
        {
            var sut = InvariantPartitioning.Instance;

            Assert.Equal("iv", sut.Master.Key);
            Assert.Equal("Invariant", sut.Master.Name);

            Assert.False(sut.Master.Fallback.Any());
            Assert.False(sut.Master.IsOptional);
        }
    }
}
