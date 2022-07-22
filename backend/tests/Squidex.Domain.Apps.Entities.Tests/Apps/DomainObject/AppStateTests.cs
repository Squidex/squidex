// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public class AppStateTests
    {
        [Fact]
        public void Should_deserialize_state()
        {
            var json = File.ReadAllText("Apps/DomainObject/AppState.json");

            var deserialized = TestUtils.DefaultSerializer.Deserialize<AppDomainObject.State>(json);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public void Should_serialize_deserialize_state()
        {
            var json = File.ReadAllText("Apps/DomainObject/AppState.json").CleanJson();

            var serialized = TestUtils.DeserializeAndSerialize<AppDomainObject.State>(json);

            Assert.Equal(json, serialized);
        }
    }
}
