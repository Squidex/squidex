﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPlanTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var plan = new AppPlan(new RefToken("user", "Me"), "free");

            var serialized = plan.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(plan);
        }
    }
}
