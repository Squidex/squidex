// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Queries;

namespace Squidex.EntityFramework.Infrastructure.Queries;

public abstract class SqlQueryTests<TContext> where TContext : DbContext
{
    protected abstract Task<TContext> CreateDbContextAsync();

    protected abstract SqlDialect CreateDialect();

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
                Text = $"Prefix{i}Suffix",
                Json = new TestJson
                {
                    Boolean = i > 10,
                    BooleanOrNull = i > 10 ? true : null,
                    Mixed = mixed,
                    Number = i,
                    NumberOrNull = i > 10 ? null : i,
                    Array = [0, i],
                    Text = $"Prefix{i}Suffix",
                },
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
    public async Task Should_sort_by_text()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Text", SortOrder.Descending)],
        });

        Assert.Equal([.. Range(9, 3), 2, 20, 1, .. Range(19, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_text_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.text", SortOrder.Descending)],
        });

        Assert.Equal([.. Range(9, 3), 2, 20, 1, .. Range(19, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_number()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Number", SortOrder.Descending)],
        });

        Assert.Equal(Range(20, 1), actual);
    }

    [Fact]
    public async Task Should_sort_by_number_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.number", SortOrder.Descending)],
        });

        Assert.Equal(Range(20, 1), actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Boolean", SortOrder.Descending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(11, 20), .. Range(1, 10)], actual);
    }

    [Fact]
    public async Task Should_sort_by_boolean()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.boolean", SortOrder.Descending), new SortNode("Number", SortOrder.Ascending)],
        });

        Assert.Equal([.. Range(11, 20), .. Range(1, 10)], actual);
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
    public async Task Should_filter_by_number()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Number", 5),
        });

        Assert.Equal(Range(6, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.number", 5),
        });

        Assert.Equal(Range(6, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_number_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Json.mixed", 5),
        });

        Assert.Equal([8, 14, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_null()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("NumberOrNull", ClrValue.Null),
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
    public async Task Should_filter_by_null_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", ClrValue.Null),
        });

        Assert.Equal([6, 12, 18], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_with_many()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Number", new List<int> { 3, 5, 7 }),
        });

        Assert.Equal([3, 5, 7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_with_many_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Json.number", new List<int> { 3, 5, 7 }),
        });

        Assert.Equal([3, 5, 7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_with_many_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Json.mixed", new List<int> { 2, 8, 14 }),
        });

        Assert.Equal([2, 8, 14], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_in_json_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.array.1", 5), ClrFilter.Lt("Json.array.1", 16)),
        });

        Assert.Equal(Range(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_in_mixed_json_array()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.mixed.0", 5), ClrFilter.Lt("Json.mixed.0", 16)),
        });

        Assert.Equal([8, 10, 14], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Boolean", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.boolean", true),
        });

        Assert.Equal(Range(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_boolean_in_mixed_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.mixed", true),
        });

        Assert.Equal([3, 9, 15], actual.Order().ToArray());
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
    public async Task Should_filter_by_string_contains_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.text", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_startsWith()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Text", "Prefix5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_startsWith_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.text", "Prefix5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_endWith()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Text", "5Suffix"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_filter_by_string_endWith_in_json()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Json.text", "5Suffix"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
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
        var builder =
            new TestSqlBuilder(CreateDialect(), "TestEntity")
                .WithLimit(query)
                .WithOffset(query)
                .WithOrders(query)
                .WithFilter(query);

        var (sql, parameters) = builder.Compile();

        var dbContext = await CreateAndPrepareDbContextAsync();
        var dbResult = await dbContext.Set<TestEntity>().FromSqlRaw(sql, parameters).ToListAsync();

        return dbResult.Select(x => x.Number).ToList();
    }
}
