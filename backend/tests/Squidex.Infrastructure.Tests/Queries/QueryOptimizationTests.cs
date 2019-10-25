﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public class QueryOptimizationTests
    {
        [Fact]
        public void Should_not_convert_optimize_valid_logical_filter()
        {
            var source = ClrFilter.Or(ClrFilter.Eq("path", 2), ClrFilter.Eq("path", 3));

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Equal("(path == 2 || path == 3)", result!.ToString());
        }

        [Fact]
        public void Should_return_filter_When_logical_filter_has_one_child()
        {
            var source = ClrFilter.And(ClrFilter.Eq("path", 1), ClrFilter.Or());

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Equal("path == 1", result!.ToString());
        }

        [Fact]
        public void Should_return_null_when_filters_of_logical_filter_get_optimized_away()
        {
            var source = ClrFilter.And(ClrFilter.And());

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_when_logical_filter_has_no_filter()
        {
            var source = ClrFilter.And();

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_when_filter_of_negation_get_optimized_away()
        {
            var source = ClrFilter.Not(ClrFilter.And());

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Null(result);
        }

        [Fact]
        public void Should_invert_equals_not_filter()
        {
            var source = ClrFilter.Not(ClrFilter.Eq("path", 1));

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Equal("path != 1", result!.ToString());
        }

        [Fact]
        public void Should_invert_notequals_not_filter()
        {
            var source = ClrFilter.Not(ClrFilter.Ne("path", 1));

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Equal("path == 1", result!.ToString());
        }

        [Fact]
        public void Should_not_convert_number_operator()
        {
            var source = ClrFilter.Not(ClrFilter.Lt("path", 1));

            var result = Optimizer<ClrValue>.Optimize(source);

            Assert.Equal("!(path < 1)", result!.ToString());
        }
    }
}
