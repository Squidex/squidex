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

public abstract class SqlQueryTests
{
    protected abstract Task<TestDbContext> CreateDbContextAsync();

    protected abstract SqlDialect CreateDialect();

    private class TestSqlBuilder(SqlDialect dialect) : SqlQueryBuilder(dialect)
    {
        public override bool IsJsonPath(PropertyPath path)
        {
            return path[0] == "Json";
        }
    }

    private async Task<TestDbContext> CreateAndPrepareDbContextAsync()
    {
        var dbContext = await CreateDbContextAsync();

        var set = dbContext.Set<TestEntity>();
        if (await set.AnyAsync())
        {
            return dbContext;
        }

        for (var i = 1; i <= 20; i++)
        {
            set.Add(new TestEntity
            {
                Boolean = i > 10,
                Number = i,
                Nullable = i > 10 ? null : i,
                Text = $"{i}",
                Json = new TestJson
                {
                    Boolean = i > 10,
                    Number = i,
                    Nullable = i > 10 ? null : i,
                    Array = [0, i],
                    Text = $"{i}",
                },
            });
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task Should_query_all()
    {
        var actual = await QueryAsync(new ClrQuery());

        Assert.Equal(CreateExpected(1, 20), actual.Order().ToArray());
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

        Assert.Equal(CreateExpected(1, 5), actual);
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

        Assert.Equal(CreateExpected(16, 20), actual);
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

        Assert.Equal(CreateExpected(6, 15), actual);
    }

    [Fact]
    public async Task Should_query_with_sorting()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Number", SortOrder.Descending)],
        });

        Assert.Equal(CreateExpected(1, 20).OrderDescending().ToArray(), actual);
    }

    [Fact]
    public async Task Should_query_with_json_sorting()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Sort = [new SortNode("Json.Number", SortOrder.Descending)],
        });

        Assert.Equal(CreateExpected(1, 20).OrderDescending().ToArray(), actual);
    }

    [Fact]
    public async Task Should_query_by_or_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Or(ClrFilter.Lt("Number", 3), ClrFilter.Gt("Number", 17)),
        });

        Assert.Equal([1, 2, 18, 19, 20], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_not_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Not(ClrFilter.Gt("Number", 10)),
        });

        Assert.Equal(CreateExpected(1, 10), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_number_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Gt("Number", 5),
        });

        Assert.Equal(CreateExpected(6, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_nullable_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Nullable", ClrValue.Null),
        });

        Assert.Equal(CreateExpected(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_in_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Number", new List<int> { 3, 5, 7 }),
        });

        Assert.Equal([3, 5, 7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_in_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.In("Json.Number", new List<int> { 3, 5, 7 }),
        });

        Assert.Equal([3, 5, 7], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_nullable_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.Nullable", ClrValue.Null),
        });

        Assert.Equal(CreateExpected(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_number_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.Number", 5), ClrFilter.Lt("Json.Number", 16)),
        });

        Assert.Equal(CreateExpected(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_array_number_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.And(ClrFilter.Gt("Json.Array.1", 5), ClrFilter.Lt("Json.Array.1", 16)),
        });

        Assert.Equal(CreateExpected(6, 15), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_boolean_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Boolean", true),
        });

        Assert.Equal(CreateExpected(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_boolean_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Eq("Json.Boolean", true),
        });

        Assert.Equal(CreateExpected(11, 20), actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_string_contains_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Text", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_string_contains_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.Contains("Json.Text", "7"),
        });

        Assert.Equal([7, 17], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_string_starts_with_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Text", "5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_string_starts_with_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.StartsWith("Json.Text", "5"),
        });

        Assert.Equal([5], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_string_ends_with_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Text", "5"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_by_json_string_ends_with_filter()
    {
        var actual = await QueryAsync(new ClrQuery
        {
            Filter = ClrFilter.EndsWith("Json.Text", "5"),
        });

        Assert.Equal([5, 15], actual.Order().ToArray());
    }

    private static long[] CreateExpected(int from, int to)
    {
        return Enumerable.Range(from, to - from + 1).Select(x => (long)x).ToArray();
    }

    private async Task<List<long>> QueryAsync(ClrQuery query)
    {
        var builder = new TestSqlBuilder(CreateDialect());

        var (sql, parameters) = builder.BuildQuery("TestEntity", query);

        var dbContext = await CreateAndPrepareDbContextAsync();
        var dbResult = await dbContext.Set<TestEntity>().FromSqlRaw(sql, parameters).ToListAsync();

        return dbResult.Select(x => x.Number).ToList();
    }
}
