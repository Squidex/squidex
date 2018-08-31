// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.ElasticSearch;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class ElasticSearchActionTests
    {
        [Fact]
        public void Should_add_error_if_host_is_null()
        {
            var action = new ElasticSearchAction { Host = null, IndexName = "squidex", IndexType = "squidex" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Host field is required.", "Host")
                });
        }

        [Fact]
        public void Should_add_error_if_host_is_relative()
        {
            var action = new ElasticSearchAction { Host = new Uri("/rel", UriKind.Relative), IndexName = "squidex", IndexType = "squidex" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Host field must be an absolute URL.", "Host")
                });
        }

        [Fact]
        public void Should_add_error_if_index_name_is_null()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = null, IndexType = "squidex" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Index Name field is required.", "IndexName")
                });
        }

        [Fact]
        public void Should_add_error_if_index_type_is_null()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = "squidex", IndexType = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Index Type field is required.", "IndexType")
                });
        }

        [Fact]
        public void Should_not_add_error_if_values_are_valid()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = "squidex", IndexType = "squidex" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
