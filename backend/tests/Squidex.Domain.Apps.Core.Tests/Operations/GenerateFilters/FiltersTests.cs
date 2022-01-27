// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.GenerateFilters
{
    public class FiltersTests
    {
        [Fact]
        public void Should_build_content_query_model()
        {
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var queryModel = ContentQueryModel.Build(TestUtils.MixedSchema(), languagesConfig.ToResolver(), ResolvedComponents.Empty);

            Assert.NotNull(queryModel);
        }

        [Fact]
        public void Should_build_dynamic_content_query_model()
        {
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var queryModel = ContentQueryModel.Build(null, languagesConfig.ToResolver(), ResolvedComponents.Empty);

            Assert.NotNull(queryModel);
        }

        [Fact]
        public void Should_build_asset_query_model()
        {
            var queryModel = AssetQueryModel.Build();

            Assert.NotNull(queryModel);
        }
    }
}
