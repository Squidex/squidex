// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Squidex.Infrastructure.Orleans;
using Xunit;

#pragma warning disable RECS0026 // Possible unassigned object created by 'new'

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetDomainObjectGrainTests
    {
        private readonly IActivationLimit limit = A.Fake<IActivationLimit>();

        [Fact]
        public void Should_set_limit()
        {
            new AssetDomainObjectGrain(A.Fake<IServiceProvider>(), limit);

            A.CallTo(() => limit.SetLimit(5000, TimeSpan.FromMinutes(5)))
                .MustHaveHappened();
        }
    }
}
