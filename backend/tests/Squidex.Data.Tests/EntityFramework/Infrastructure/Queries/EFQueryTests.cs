// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NetTopologySuite.Geometries;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.EntityFramework.Infrastructure.Queries;

public abstract class EFQueryTests<TContext>(ISqlFixture<TContext> fixture)
    where TContext : DbContext, IDbContextWithDialect
{
    protected Task<TContext> CreateDbContextAsync()
    {
        return fixture.DbContextFactory.CreateDbContextAsync();
    }

    private class TestSqlBuilder(SqlDialect dialect, string table) : SqlQueryBuilder(dialect, table)
    {
        public override bool IsJsonPath(PropertyPath path)
        {
            return path[0] == "Json";
        }
    }

    private async Task<TContext> CreateAndPrepareDbContextAsync()
    {
        var dbContext = await CreateDbContextAsync();

        await dbContext.CreateGeoIndexAsync("IDX_GEO", nameof(TestEntity), "Point");
        await dbContext.CreateTextIndexAsync("IDX_Text", nameof(TestEntity), "FullText");

        var set = dbContext.Set<TestEntity>();
        if (await set.AnyAsync())
        {
            return dbContext;
        }

        for (var i = 1; i <= 20; i++)
        {
            object? mixed;
            switch (i % 6)
            {
                case 0:
                    mixed = null;
                    break;
                case 1:
                    mixed = $"Prefix{i}Suffix";
                    break;
                case 2:
                    mixed = i;
                    break;
                case 3:
                    mixed = true;
                    break;
                case 4:
                    mixed = new List<object> { i };
                    break;
                default:
                    mixed = new Dictionary<string, object>();
                    break;
            }

            set.Add(new TestEntity
            {
                Boolean = i > 10,
                BooleanOrNull = i > 10 ? true : null,
                Number = i,
                NumberOrNull = i > 10 ? null : i,
                FullText = "hello world",
                Text = $"Prefix{i}Suffix",
                Json = new TestJson
                {
                    Boolean = i > 10,
                    BooleanArray = [i > 10, i > 15],
                    BooleanOrNull = i > 10 ? true : null,
                    Mixed = mixed,
                    MixedArray = [i, i % 3 == 0 ? null : $"T{i}", i > 10],
                    Number = i,
                    NumberArray = [0, i],
                    NumberOrNull = i > 10 ? null : i,
                    Text = $"Prefix{i}Suffix",
                    TextArray = [$"X{i}", $"Y{i}"],
                    TextOrNull = i > 10 ? null : $"Prefix{i}Suffix",
                },
                Point = new Point(i * 2, i * 2) { SRID = 4326 },
            });
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task Should_query()
    {
        var actual = await QueryAsync(new ClrQuery());

        Assert.Equal(Range(1, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_with_take()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Take = 5,
            Skip = 0,
            Sort = [new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal(Range(1, 5), actual);
    }

    [Fact]
    public async Task Should_query_with_skip()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Take = long.MaxValue,
            Skip = 15,
            Sort = [new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal(Range(16, 20), actual);
    }

    [Fact]
    public async Task Should_query_with_skip_and_take()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Take = 10,
            Skip = 5,
            Sort = [new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal(Range(6, 15), actual);
    }

    [Fact]
    public async Task Should_query_with_skip_and_take_sorted_descending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Take = 5,
            Skip = 5,
            Sort = [new SortNode("Number", SortOrder.Descending)],
        });

        Assert.Equal(Range(15, 11), actual);
    }

    [Fact]
    public async Task Should_sort_by_number_ascending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal(Range(1, 20), actual);
    }

    [Fact]
    public async Task Should_sort_by_number_descending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Number", SortOrder.Descending)],
        });

        Assert.Equal(Range(20, 1), actual);
    }

    [Fact]
    public async Task Should_sort_by_number_ascending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.number", SortOrder.Ascending)],
        });

        Assert.Equal(Range(1, 20), actual);
    }

    [Fact]
    public async Task Should_sort_by_number_descending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.number", SortOrder.Descending)],
        });

        Assert.Equal(Range(20, 1), actual);
    }

    [Fact]
    public async Task Should_sort_by_text_ascending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Text", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(10, 19), 1, 2, 20, .. Range(3, 9)], actual);
    }

    [Fact]
    public async Task Should_sort_by_text_descending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Text", SortOrder.Descending)],
        });

        Assert.Equal([.. Range(9, 3), 2, 20, 1, .. Range(19, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_text_ascending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.text", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(10, 19), 1, 2, 20, .. Range(3, 9)], actual);
    }

    [Fact]
    public async Task Should_sort_by_text_descending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.text", SortOrder.Descending)],
        });

        Assert.Equal([.. Range(9, 3), 2, 20, 1, .. Range(19, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean_ascending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Boolean", SortOrder.Ascending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(1, 10), .. Range(11, 20)], actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean_descending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Boolean", SortOrder.Descending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(11, 20), .. Range(1, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean_ascending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.boolean", SortOrder.Ascending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(1, 10), .. Range(11, 20)], actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean_descending_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.boolean", SortOrder.Descending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(11, 20), .. Range(1, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_mixed_json_descending()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.mixed", SortOrder.Descending)],
        });

        Assert.Equal([20, 14, 8, 2], actual.Take(4));
    }

    [Fact]
    public async Task Should_sort_by_multiple_fields()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort =
            [
                new SortNode("Boolean", SortOrder.Ascending),
                new SortNode("Number", SortOrder.Descending),
            ],
        });

        Assert.Equal([.. Range(10, 1), .. Range(20, 11)], actual);
    }

    [Fact]
    public async Task Should_filter_with_or()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Or(ClrFilter.Lt("Number", 3), ClrFilter.Gt("Number", 17)),
        });

        Assert.Equal([1, 2, 18, 19, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_with_and()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.number", 5), ClrFilter.Lt("Json.number", 16)),
        });

        Assert.Equal(Range(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_with_not()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Not(ClrFilter.Gt("Number", 10)),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Number", 7),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_not_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Number", 7),
        });

        Assert.Equal(AllExept(7), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Number", 15),
        });

        Assert.Equal(Range(16, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_or_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ge("Number", 15),
        });

        Assert.Equal(Range(15, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Lt("Number", 5),
        });

        Assert.Equal(Range(1, 4), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than_or_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Le("Number", 5),
        });

        Assert.Equal(Range(1, 5), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.number", 7),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_not_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.number", 7),
        });

        Assert.Equal(AllExept(7), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.number", 15),
        });

        Assert.Equal(Range(16, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_or_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ge("Json.number", 15),
        });

        Assert.Equal(Range(15, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Lt("Json.number", 5),
        });

        Assert.Equal(Range(1, 4), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than_or_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Le("Json.number", 5),
        });

        Assert.Equal(Range(1, 5), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.numberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_not_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.numberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.numberOrNull", 5),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.numberOrNull", 7),
        });

        Assert.Equal([8, 9, 10], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Lt("Json.numberOrNull", 4),
        });

        Assert.Equal([1, 2, 3], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", 8),
        });

        Assert.Equal([8], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.mixed", 5),
        });

        Assert.Equal([8, 10, 14, 16, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Json.mixed", new List<int> { 2, 8, 14 }),
        });

        Assert.Equal([2, 8, 14], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal_in_json_number_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.numberArray", 7),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_in_json_number_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.numberArray", 18),
        });

        Assert.Equal([19, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_or_equal_in_json_number_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ge("Json.numberArray", 18),
        });

        Assert.Equal([18, 19, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_in_json_number_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Json.numberArray", new List<int> { 3, 5, 7 }),
        });

        Assert.Equal([3, 5, 7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_with_index_in_json_number_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.numberArray.1", 5), ClrFilter.Lt("Json.numberArray.1", 16)),
        });

        Assert.Equal(Range(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_equal_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixedArray", 7),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_greater_than_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.mixedArray", 16),
        });

        Assert.Equal([17, 18, 19, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_less_than_or_equal_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Le("Json.mixedArray", 3),
        });

        Assert.Equal([1, 2, 3], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_with_index_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.mixedArray.0", 5), ClrFilter.Lt("Json.mixedArray.0", 16)),
        });

        Assert.Equal(Range(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Text", "Prefix7Suffix"),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_not_equal()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Text", "Prefix7Suffix"),
        });

        Assert.Equal(AllExept(7), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_contains()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Text", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Text", "Prefix5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_ends_with()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Text", "5Suffix"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.text", "Prefix7Suffix"),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_not_equal_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.text", "Prefix7Suffix"),
        });

        Assert.Equal(AllExept(7), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_contains_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.text", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.text", "Prefix5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_ends_with_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Json.text", "5Suffix"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.textOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_not_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.textOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_equal_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.textOrNull", "Prefix5Suffix"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_contains_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.textOrNull", "5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.textOrNull", "Prefix"),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_contains_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.mixed", "7"),
        });

        Assert.Equal([7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.mixed", "Prefix"),
        });

        Assert.Equal([1, 7, 13, 19], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_ends_with_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Json.mixed", "Suffix"),
        });

        Assert.Equal([1, 7, 13, 19], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_equal_in_json_text_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.textArray", "X5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_not_equal_in_json_text_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.textArray", "X5"),
        });

        Assert.Equal(20, actual.Count);
    }

    [Fact]
    public async Task Should_filter_by_string_contains_in_json_text_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.textArray", "5"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with_in_json_text_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.textArray", "X5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_ends_with_in_json_text_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Json.textArray", "5"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_contains_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.mixedArray", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_starts_with_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.mixedArray", "T5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_true()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Boolean", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_false()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Boolean", false),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_true_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.boolean", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_false_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.boolean", false),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.booleanOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_not_null_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.booleanOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_true_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.booleanOrNull", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_true_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", true),
        });

        Assert.Equal([3, 9, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_false_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", false),
        });

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_filter_by_boolean_true_in_json_boolean_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.booleanArray", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_false_in_json_boolean_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.booleanArray", false),
        });

        Assert.Equal(Range(1, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_true_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixedArray", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_false_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixedArray", false),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_on_number()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("NumberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_on_number()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("NumberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_on_boolean()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("BooleanOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_on_boolean()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("BooleanOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.numberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.numberOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_on_text_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.textOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_on_text_in_nullable_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.textOrNull", ClrValue.Null),
        });

        Assert.Equal(Range(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", ClrValue.Null),
        });

        Assert.Equal([6, 12, 18], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.mixed", ClrValue.Null),
        });

        Assert.Equal(AllExept(6, 12, 18), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixedArray", ClrValue.Null),
        });

        Assert.Equal([3, 6, 9, 12, 15, 18], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_not_null_in_json_mixed_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Ne("Json.mixedArray", ClrValue.Null),
        });

        Assert.Equal(20, actual.Count);
    }

    [Fact]
    public async Task Should_query_count()
    {
        var dbContext = await CreateAndPrepareDbContextAsync();

        var builder =
            new TestSqlBuilder(dbContext.Dialect, nameof(TestEntity))
                .Count();

        var (sql, parameters) = builder.Compile();
        var dbResult = await dbContext.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();

        Assert.Equal(20, dbResult);
    }

    [Fact]
    public async Task Should_query_count_with_filter()
    {
        var dbContext = await CreateAndPrepareDbContextAsync();

        var query = new ClrQuery
        {
            Filter = ClrFilter.Gt("Number", 10),
        };

        var builder =
            new TestSqlBuilder(dbContext.Dialect, nameof(TestEntity))
                .Count()
                .Where(query);

        var (sql, parameters) = builder.Compile();
        var dbResult = await dbContext.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();

        Assert.Equal(10, dbResult);
    }

    [Fact]
    public async Task Should_query_by_distance()
    {
        var point = new Point(4, 4) { SRID = 4326 };

        var dbContext = await CreateAndPrepareDbContextAsync();
        var dbResult = await dbContext.Set<TestEntity>().Where(x => x.Point.Distance(point) < 1).ToListAsync();

        Assert.Single(dbResult);
    }

    [Fact]
    public async Task Should_query_full_text()
    {
        var dbContext = await CreateAndPrepareDbContextAsync();

        var queryBuilder =
            new TestSqlBuilder(dbContext.Dialect, nameof(TestEntity))
                .WhereMatch("FullText", "hello");

        var (sql, parameters) = queryBuilder.Compile();
        var dbResult = await PollAsync(dbContext, sql, parameters, 20);

        Assert.Equal(20, dbResult.Count);
    }

    [Fact]
    public async Task Should_query_full_text_with_space()
    {
        var dbContext = await CreateAndPrepareDbContextAsync();

        var queryBuilder =
            new TestSqlBuilder(dbContext.Dialect, nameof(TestEntity))
                .WhereMatch("FullText", "hello world");

        var (sql, parameters) = queryBuilder.Compile();
        var dbResult = await PollAsync(dbContext, sql, parameters, 0);

        Assert.Empty(dbResult);
    }

    private static long[] AllExept(params long[] values)
    {
        return Range(1, 20).Except(values).ToArray();
    }

    private static long[] Range(int from, int to)
    {
        var result = new List<long>();
        if (to >= from)
        {
            for (var i = from; i <= to; i++)
            {
                result.Add(i);
            }
        }
        else
        {
            for (var i = from; i >= to; i--)
            {
                result.Add(i);
            }
        }

        return result.ToArray();
    }

    private async Task<List<long>> QueryAsync(ClrQuery query)
    {
        var dbContext = await CreateAndPrepareDbContextAsync();

        var queryBuilder =
            new TestSqlBuilder(dbContext.Dialect, nameof(TestEntity))
                .Limit(query)
                .Offset(query)
                .Order(query)
                .Where(query);

        var (sql, parameters) = queryBuilder.Compile();
        File.WriteAllText("D:\\last.json", sql);
        var dbResult = await dbContext.Set<TestEntity>().FromSqlRaw(sql, parameters).ToListAsync();

        return dbResult.Select(x => x.Number).ToList();
    }

    private static async Task<List<TestEntity>> PollAsync(TContext dbContext, string sql, object[] parameters, int expectedCount)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        while (!cts.IsCancellationRequested)
        {
            var dbResult = await dbContext.Set<TestEntity>().FromSqlRaw(sql, parameters).ToListAsync(default);
            if (dbResult.Count == expectedCount)
            {
                return dbResult;
            }

            await Task.Delay(50, default);
        }

        return [];
    }
}
