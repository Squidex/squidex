// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class LanguagesConfigJsonTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = LanguagesConfig.Build(
                new LanguageConfig(Language.EN),
                new LanguageConfig(Language.DE, true, Language.EN),
                new LanguageConfig(Language.IT, false, Language.DE))
                .MakeMaster(Language.IT);

            var serialized = JToken.FromObject(sut, serializer).ToObject<LanguagesConfig>(serializer);

            serialized.Should().BeEquivalentTo(sut);

            Assert.Same(serialized.FirstOrDefault(x => x.Key == "it"), serialized.Master);
        }
    }
}
