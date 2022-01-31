﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.GenerateFilters
{
    public class FiltersTests
    {
        [Fact]
        public void Should_build_content_query_model()
        {
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var (schema, components) = TestUtils.MixedSchema();

            var queryModel = ContentQueryModel.Build(schema, languagesConfig.ToResolver(), components);

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

        private static void CheckFields(FilterSchema filterSchema, Schema schema)
        {
            var filterProperties = AllPropertyNames(filterSchema);

            void CheckField(IField field)
            {
                if (!field.IsForApi())
                {
                    Assert.DoesNotContain(field.Name, filterProperties);
                }
                else
                {
                    Assert.Contains(field.Name, filterProperties);
                }

                if (field is IArrayField array)
                {
                    foreach (var nested in array.Fields)
                    {
                        CheckField(nested);
                    }
                }
            }

            foreach (var field in schema.Fields)
            {
                CheckField(field);
            }
        }

        private static HashSet<string> AllPropertyNames(FilterSchema schema)
        {
            var result = new HashSet<string>();

            void AddProperties(FilterSchema current)
            {
                if (current == null)
                {
                    return;
                }

                foreach (var field in current.Fields.OrEmpty())
                {
                    result.Add(field.Path);

                    AddProperties(field.Schema);
                }
            }

            AddProperties(schema);

            return result;
        }
    }
}
