// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using ExtensionSut = Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations.Extensions;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public class ExtensionsTests
{
    public ExtensionsTests()
    {
        MongoContentEntity.RegisterClassMap();
    }

    [Fact]
    public void Should_build_projection_without_fields()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(null);

        AssertProjection(projection, "{ 'dd' : 0 }");
    }

    [Fact]
    public void Should_build_projection_with_data_prefix()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(["data.myField"]);

        AssertProjection(projection, "{ '_ai' : 1, '_id' : 1, '_si' : 1, 'ai' : 1, 'cb' : 1, 'ct' : 1, 'dl' : 1, 'do.myField' : 1, 'id' : 1, 'is' : 1, 'mb' : 1, 'mt' : 1, 'ns' : 1, 'rf' : 1, 'sa' : 1, 'si' : 1, 'sj' : 1, 'ss' : 1, 'ts' : 1, 'vs' : 1 }");
    }

    [Fact]
    public void Should_build_projection_without_data_prefix()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(["myField"]);

        AssertProjection(projection, "{ '_ai' : 1, '_id' : 1, '_si' : 1, 'ai' : 1, 'cb' : 1, 'ct' : 1, 'dl' : 1, 'do.myField' : 1, 'id' : 1, 'is' : 1, 'mb' : 1, 'mt' : 1, 'ns' : 1, 'rf' : 1, 'sa' : 1, 'si' : 1, 'sj' : 1, 'ss' : 1, 'ts' : 1, 'vs' : 1 }");
    }

    [Fact]
    public void Should_build_projection_without_included_field()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(["myField.special", "myField"]);

        AssertProjection(projection, "{ '_ai' : 1, '_id' : 1, '_si' : 1, 'ai' : 1, 'cb' : 1, 'ct' : 1, 'dl' : 1, 'do.myField' : 1, 'id' : 1, 'is' : 1, 'mb' : 1, 'mt' : 1, 'ns' : 1, 'rf' : 1, 'sa' : 1, 'si' : 1, 'sj' : 1, 'ss' : 1, 'ts' : 1, 'vs' : 1 }");
    }

    [Fact]
    public void Should_build_projection_with_status_data_field()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(["data.Status"]);

        AssertProjection(projection, "{ '_ai' : 1, '_id' : 1, '_si' : 1, 'ai' : 1, 'cb' : 1, 'ct' : 1, 'dl' : 1, 'do.Status' : 1, 'id' : 1, 'is' : 1, 'mb' : 1, 'mt' : 1, 'ns' : 1, 'rf' : 1, 'sa' : 1, 'si' : 1, 'sj' : 1, 'ss' : 1, 'ts' : 1, 'vs' : 1 }");
    }

    [Fact]
    public void Should_build_projection_with_meta_status_field()
    {
        var projection = ExtensionSut.BuildProjection2<MongoContentEntity>(["status"]);

        AssertProjection(projection, "{ '_ai' : 1, '_id' : 1, '_si' : 1, 'ai' : 1, 'cb' : 1, 'ct' : 1, 'dl' : 1, 'do.status' : 1, 'id' : 1, 'is' : 1, 'mb' : 1, 'mt' : 1, 'ns' : 1, 'rf' : 1, 'sa' : 1, 'si' : 1, 'sj' : 1, 'ss' : 1, 'ts' : 1, 'vs' : 1 }");
    }

    private static void AssertProjection(ProjectionDefinition<MongoContentEntity, MongoContentEntity> projection, string expected)
    {
        var rendered =
            projection.Render(
                BsonSerializer.SerializerRegistry.GetSerializer<MongoContentEntity>(),
                BsonSerializer.SerializerRegistry)
            .Document.ToString();

        Assert.Equal(Cleanup(expected), rendered);
    }

    private static string Cleanup(string filter)
    {
        return filter.Replace('\'', '"');
    }
}
