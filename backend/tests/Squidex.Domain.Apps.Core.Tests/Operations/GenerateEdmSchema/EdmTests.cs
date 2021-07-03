// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
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
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var typeFactory = new EdmTypeFactory(names =>
            {
                return (new EdmComplexType("Squidex", string.Join(".", names)), true);
            });

            var edmModel =
                TestUtils.MixedSchema()
                    .BuildEdmType(true, languagesConfig.ToResolver(), typeFactory, ResolvedComponents.Empty);

            Assert.NotNull(edmModel);
        }
    }
}
