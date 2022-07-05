// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

#pragma warning disable CA1806 // Do not ignore method results

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public class AssetFolderDomainObjectGrainTests
    {
        private readonly IActivationLimit limit = A.Fake<IActivationLimit>();

        [Fact]
        public void Should_set_limit()
        {
            var id = DomainId.NewGuid();

            var identity = A.Fake<IGrainIdentity>();

            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(id.ToString());

            var factory = A.Fake<IDomainObjectFactory>();

            A.CallTo(() => factory.Create<AssetFolderDomainObject>(id))
                .Returns(A.Dummy<AssetFolderDomainObject>());

            new AssetFolderDomainObjectGrain(identity, factory, limit);

            A.CallTo(() => limit.SetLimit(5000, TimeSpan.FromMinutes(5)))
                .MustHaveHappened();
        }
    }
}
