// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;
using ClrFilter = Squidex.Infrastructure.Queries.ClrFilter;
using SortBuilder = Squidex.Infrastructure.Queries.SortBuilder;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

public class AssetQueryTests
{
    private readonly DomainId appId = DomainId.NewGuid();

    static AssetQueryTests()
    {
        TestUtils.SetupBson();
    }

    [Fact]
    public void Should_throw_exception_for_full_text_search()
    {
        Assert.Throws<ValidationException>(() => AssertQuery(string.Empty, new ClrQuery { FullText = "Full Text" }));
    }

    [Fact]
    public void Should_make_query_with_id()
    {
        var id = Guid.NewGuid();

        var filter = ClrFilter.Eq("id", id);

        AssertQuery($"{{ '_id' : '{appId}--{id}' }}", filter);
    }

    [Fact]
    public void Should_make_query_with_id_string()
    {
        var id = DomainId.NewGuid().ToString();

        var filter = ClrFilter.Eq("id", id);

        AssertQuery($"{{ '_id' : '{appId}--{id}' }}", filter);
    }

    [Fact]
    public void Should_make_query_with_id_list()
    {
        var id = Guid.NewGuid();

        var filter = ClrFilter.In("id", new List<Guid> { id });

        AssertQuery($"{{ '_id' : {{ '$in' : ['{appId}--{id}'] }} }}", filter);
    }

    [Fact]
    public void Should_make_query_with_id_string_list()
    {
        var id = DomainId.NewGuid().ToString();

        var filter = ClrFilter.In("id", new List<string> { id });

        AssertQuery($"{{ '_id' : {{ '$in' : ['{appId}--{id}'] }} }}", filter);
    }

    [Fact]
    public void Should_make_query_with_lastModified()
    {
        var time = "1988-01-19T12:00:00Z";

        var filter = ClrFilter.Eq("lastModified", InstantPattern.ExtendedIso.Parse(time).Value);

        AssertQuery("{ 'mt' : ISODate('[value]') }", filter, time);
    }

    [Fact]
    public void Should_make_query_with_lastModifiedBy()
    {
        var filter = ClrFilter.Eq("lastModifiedBy", "subject:me");

        AssertQuery("{ 'mb' : 'subject:me' }", filter);
    }

    [Fact]
    public void Should_make_query_with_created()
    {
        var time = "1988-01-19T12:00:00Z";

        var filter = ClrFilter.Eq("created", InstantPattern.ExtendedIso.Parse(time).Value);

        AssertQuery("{ 'ct' : ISODate('[value]') }", filter, time);
    }

    [Fact]
    public void Should_make_query_with_createdBy()
    {
        var filter = ClrFilter.Eq("createdBy", "subject:me");

        AssertQuery("{ 'cb' : 'subject:me' }", filter);
    }

    [Fact]
    public void Should_make_query_with_version()
    {
        var filter = ClrFilter.Eq("version", 2L);

        AssertQuery("{ 'vs' : NumberLong(2) }", filter);
    }

    [Fact]
    public void Should_make_query_with_fileVersion()
    {
        var filter = ClrFilter.Eq("fileVersion", 2L);

        AssertQuery("{ 'fv' : NumberLong(2) }", filter);
    }

    [Fact]
    public void Should_make_query_with_tags()
    {
        var filter = ClrFilter.Eq("tags", "tag1");

        AssertQuery("{ 'td' : 'tag1' }", filter);
    }

    [Fact]
    public void Should_make_query_with_fileName()
    {
        var filter = ClrFilter.Eq("fileName", "Logo.png");

        AssertQuery("{ 'fn' : 'Logo.png' }", filter);
    }

    [Fact]
    public void Should_make_query_with_mimeType()
    {
        var filter = ClrFilter.Eq("mimeType", "text/json");

        AssertQuery("{ 'mm' : 'text/json' }", filter);
    }

    [Fact]
    public void Should_make_query_with_fileSize()
    {
        var filter = ClrFilter.Eq("fileSize", 1024);

        AssertQuery("{ 'fs' : NumberLong(1024) }", filter);
    }

    [Fact]
    public void Should_make_query_with_pixelHeight()
    {
        var filter = ClrFilter.Eq("metadata.pixelHeight", 600);

        AssertQuery("{ 'md.pixelHeight' : 600 }", filter);
    }

    [Fact]
    public void Should_make_query_with_pixelWidth()
    {
        var filter = ClrFilter.Eq("metadata.pixelWidth", 800);

        AssertQuery("{ 'md.pixelWidth' : 800 }", filter);
    }

    [Fact]
    public void Should_make_orderby_with_single_field()
    {
        var sorting = SortBuilder.Descending("created");

        AssertSorting("{ 'ct' : -1 }", sorting);
    }

    [Fact]
    public void Should_make_orderby_with_multiple_fields()
    {
        var sorting1 = SortBuilder.Ascending("created");
        var sorting2 = SortBuilder.Descending("createdBy");

        AssertSorting("{ 'ct' : 1, 'cb' : -1 }", sorting1, sorting2);
    }

    private void AssertQuery(string expected, FilterNode<ClrValue> filter, object? arg = null)
    {
        AssertQuery(expected, new ClrQuery { Filter = filter }, arg);
    }

    private void AssertQuery(string expected, ClrQuery query, object? arg = null)
    {
        var filter = query.AdjustToModel(appId).BuildFilter<MongoAssetEntity>(false).Filter!;

        var rendered =
            filter.Render(
                BsonSerializer.SerializerRegistry.GetSerializer<MongoAssetEntity>(),
                BsonSerializer.SerializerRegistry)
            .ToString();

        Assert.Equal(Cleanup(expected, arg), rendered);
    }

    private void AssertSorting(string expected, params SortNode[] sort)
    {
        var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

        var rendered = string.Empty;

        A.CallTo(() => cursor.Sort(A<SortDefinition<MongoAssetEntity>>._))
            .Invokes((SortDefinition<MongoAssetEntity> sortDefinition) =>
            {
                rendered =
                   sortDefinition.Render(
                       BsonSerializer.SerializerRegistry.GetSerializer<MongoAssetEntity>(),
                       BsonSerializer.SerializerRegistry)
                   .ToString();
            });

        cursor.QuerySort(new ClrQuery { Sort = sort.ToList() }.AdjustToModel(appId));

        Assert.Equal(Cleanup(expected), rendered);
    }

    private static string Cleanup(string filter, object? arg = null)
    {
        return filter.Replace('\'', '"').Replace("[value]", arg?.ToString(), StringComparison.Ordinal);
    }
}
