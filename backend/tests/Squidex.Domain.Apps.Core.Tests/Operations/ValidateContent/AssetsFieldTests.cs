// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class AssetsFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new AssetsFieldProperties());

            Assert.Equal("my-assets", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_null_and_valid()
        {
            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        private static RootField<AssetsFieldProperties> Field(AssetsFieldProperties properties)
        {
            return Fields.Assets(1, "my-assets", Partitioning.Invariant, properties);
        }
    }
}
