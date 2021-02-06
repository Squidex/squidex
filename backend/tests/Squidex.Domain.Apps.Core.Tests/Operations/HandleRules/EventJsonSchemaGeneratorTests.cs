// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class EventJsonSchemaGeneratorTests
    {
        private readonly EventJsonSchemaGenerator sut;

        public EventJsonSchemaGeneratorTests()
        {
            var jsonSchemaGenerator =
                new JsonSchemaGenerator(
                    new JsonSchemaGeneratorSettings());

            sut = new EventJsonSchemaGenerator(jsonSchemaGenerator);
        }

        public static IEnumerable<string> AllTypes()
        {
            yield return nameof(EnrichedAssetEvent);
            yield return nameof(EnrichedCommentEvent);
            yield return nameof(EnrichedContentEvent);
            yield return nameof(EnrichedManualEvent);
            yield return nameof(EnrichedSchemaEvent);
            yield return nameof(EnrichedUsageExceededEvent);
        }

        public static IEnumerable<object[]> AllTypesData()
        {
            return AllTypes().Select(x => new object[] { x });
        }

        [Fact]
        public void Should_return_null_for_unknown_type_name()
        {
            var schema = sut.GetSchema("Unknown");

            Assert.Null(schema);
        }

        [Fact]
        public void Should_provide_all_types()
        {
            var allTypes = sut.AllTypes;

            Assert.Equal(AllTypes().ToList(), allTypes);
        }

        [Theory]
        [MemberData(nameof(AllTypesData))]
        public void Should_generate_json_schema_for_known_event(string typeName)
        {
            var schema = sut.GetSchema(typeName);

            Assert.NotNull(schema);
        }
    }
}
