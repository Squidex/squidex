// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Apps;

public class LanguagesConfigJsonTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var languages =
            LanguagesConfig.English
                .Set(Language.FR)
                .Set(Language.IT)
                .Set(Language.DE, true, Language.IT)
                .MakeMaster(Language.FR);

        var serialized = languages.SerializeAndDeserialize();

        serialized.Should().BeEquivalentTo(languages);
    }
}
