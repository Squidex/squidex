// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class LanguagesConfigJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var languages = LanguagesConfig.Build(
                new LanguageConfig(Language.EN),
                new LanguageConfig(Language.DE, true, Language.EN),
                new LanguageConfig(Language.IT, false, Language.DE))
                .MakeMaster(Language.IT);

            var serialized = languages.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(languages);

            Assert.Same(serialized.FirstOrDefault(x => x.Key == "it"), serialized.Master);
        }
    }
}
