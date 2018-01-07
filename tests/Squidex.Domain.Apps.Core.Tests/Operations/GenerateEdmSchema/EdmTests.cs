// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.GenerateEdmSchema
{
    public class EdmTests
    {
        [Fact]
        public void Should_escape_field_name()
        {
            Assert.Equal("field_name", "field-name".EscapeEdmField());
        }

        [Fact]
        public void Should_unescape_field_name()
        {
            Assert.Equal("field-name", "field_name".UnescapeEdmField());
        }

        [Fact]
        public void Should_build_edm_model()
        {
            var languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);

            var edmModel = TestData.MixedSchema().BuildEdmType(languagesConfig.ToResolver(), x => x);

            Assert.NotNull(edmModel);
        }
    }
}
