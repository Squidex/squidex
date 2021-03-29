// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class StreamMapperTests
    {
        private readonly DomainId appIdOld = DomainId.NewGuid();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly StreamMapper sut;

        public StreamMapperTests()
        {
            sut = new StreamMapper(new RestoreContext(appId,
                A.Fake<IUserMapping>(),
                A.Fake<IBackupReader>(),
                appIdOld));
        }

        [Fact]
        public void Should_map_old_app_id()
        {
            var result = sut.Map($"app-{appIdOld}");

            Assert.Equal(($"app-{appId}", appId), result);
        }

        [Fact]
        public void Should_map_old_app_broken_id()
        {
            var result = sut.Map($"app-{appIdOld}--{appIdOld}");

            Assert.Equal(($"app-{appId}", appId), result);
        }

        [Fact]
        public void Should_map_non_app_id()
        {
            var result = sut.Map($"content-{appIdOld}--123");

            Assert.Equal(($"content-{appId}--123", DomainId.Create($"{appId}--123")), result);
        }

        [Fact]
        public void Should_map_non_app_id_with_double_slash()
        {
            var result = sut.Map($"content-{appIdOld}--other--id");

            Assert.Equal(($"content-{appId}--other--id", DomainId.Create($"{appId}--other--id")), result);
        }

        [Fact]
        public void Should_map_non_combined_id()
        {
            var id = DomainId.NewGuid();

            var result = sut.Map($"content-{id}");

            Assert.Equal(($"content-{appId}--{id}", DomainId.Create($"{appId}--{id}")), result);
        }
    }
}
