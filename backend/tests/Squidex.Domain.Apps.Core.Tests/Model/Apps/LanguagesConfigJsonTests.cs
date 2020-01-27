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
    public class LanguagesConfigJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var languages =
                LanguagesConfig.English
                    .Set(Language.FR)
                    .Set(Language.IT, false)
                    .Set(Language.DE, true, new Language[] { Language.IT })
                    .MakeMaster(Language.FR);

            var serialized = languages.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(languages);
        }
    }
}
