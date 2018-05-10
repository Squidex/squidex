// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class ElasticSearchActionTests
    {
        [Fact]
        public async Task Should_add_error_if_host_is_null()
        {
            var action = new ElasticSearchAction { Host = null, IndexName = "squidex", IndexType = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_host_is_relative()
        {
            var action = new ElasticSearchAction { Host = new Uri("/rel", UriKind.Relative), IndexName = "squidex", IndexType = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_index_name_is_null()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = null, IndexType = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_index_type_is_null()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = "squidex", IndexType = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_values_are_valid()
        {
            var action = new ElasticSearchAction { Host = new Uri("http://host", UriKind.Absolute), IndexName = "squidex", IndexType = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
