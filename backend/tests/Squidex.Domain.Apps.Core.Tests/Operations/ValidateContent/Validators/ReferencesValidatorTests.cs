// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ReferencesValidatorTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid ref1 = Guid.NewGuid();
        private readonly Guid ref2 = Guid.NewGuid();

        [Fact]
        public async Task Should_add_error_if_references_are_not_valid()
        {
            var sut = new ReferencesValidator(Enumerable.Repeat(schemaId, 1), FoundReferences());

            await sut.ValidateAsync(CreateValue(ref1), errors);

            errors.Should().BeEquivalentTo(
                new[] { $"Contains invalid reference '{ref1}'." });
        }

        [Fact]
        public async Task Should_not_add_error_if_reference_are_not_valid_but_in_optimized_mode()
        {
            var sut = new ReferencesValidator(Enumerable.Repeat(schemaId, 1), FoundReferences());

            await sut.ValidateAsync(CreateValue(ref1), errors, updater: c => c.Optimized());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_not_defined()
        {
            var sut = new ReferencesValidator(null, FoundReferences((Guid.NewGuid(), ref2)));

            await sut.ValidateAsync(CreateValue(ref2), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_reference_schema_is_not_valid()
        {
            var sut = new ReferencesValidator(Enumerable.Repeat(schemaId, 1), FoundReferences((Guid.NewGuid(), ref2)));

            await sut.ValidateAsync(CreateValue(ref2), errors);

            errors.Should().BeEquivalentTo(
                new[] { $"Contains reference '{ref2}' to invalid schema." });
        }

        private static List<Guid> CreateValue(params Guid[] ids)
        {
            return ids.ToList();
        }

        private static CheckContentsByIds FoundReferences(params (Guid SchemaId, Guid Id)[] references)
        {
            return new CheckContentsByIds(x => Task.FromResult<IReadOnlyList<(Guid SchemaId, Guid Id)>>(references.ToList()));
        }
    }
}
