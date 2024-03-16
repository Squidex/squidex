// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class AppPlanTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var plan = new AppPlan(RefToken.Client("Me"), "free");

            var serialized = plan.SerializeAndDeserialize();

            Assert.Equal(plan, serialized);
        }
    }
}
