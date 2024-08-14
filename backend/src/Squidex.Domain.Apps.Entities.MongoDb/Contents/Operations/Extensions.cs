// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

public static class Extensions
{
    private static readonly BsonDocument LookupLet =
        new BsonDocument()
            .Add("id", "$_id");

    private static readonly BsonDocument LookupMatch =
        new BsonDocument()
            .Add("$expr", new BsonDocument()
                .Add("$eq", new BsonArray { "$_id", "$$id" }));

    private static Dictionary<string, string> metaFields;

    private static IReadOnlyDictionary<string, string> MetaFields
    {
        get => metaFields ??=
            BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).AllMemberMaps
                .Where(x =>
                    x.MemberName != nameof(MongoContentEntity.NewData) &&
                    x.MemberName != nameof(MongoContentEntity.Data))
                .ToDictionary(
                    x => x.MemberName,
                    x => x.ElementName,
                    StringComparer.OrdinalIgnoreCase);
    }

    public sealed class StatusOnly
    {
        [BsonId]
        [BsonElement("_id")]
        public DomainId DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("id")]
        public DomainId Id { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        public DomainId IndexedSchemaId { get; set; }

        [BsonRequired]
        [BsonElement("ss")]
        public Status Status { get; set; }
    }

    public sealed class IdOnly
    {
        [BsonId]
        [BsonElement("_id")]
        public DomainId Id { get; set; }

        [BsonElement(nameof(Joined))]
        public MongoContentEntity[] Joined { get; set; }
    }

    public static bool IsSatisfiedByIndex(this ClrQuery query)
    {
        return
            query.Sort is { Count: 2 } &&
            query.Sort[0].Path.ToString() == "mt" &&
            query.Sort[0].Order == SortOrder.Descending &&
            query.Sort[1].Path.ToString() == "id" &&
            query.Sort[1].Order == SortOrder.Ascending;
    }

    public static async Task<List<MongoContentEntity>> QueryContentsAsync(this IMongoCollection<MongoContentEntity> collection, FilterDefinition<MongoContentEntity> filter, ClrQuery query, Q q,
        CancellationToken ct)
    {
        if (query.Skip > 0 && !query.IsSatisfiedByIndex())
        {
            // If we have to skip over items, we could reach the limit of the sort buffer, therefore get the ids and all filter fields only
            // in a first iteration and get the actual content in the a second query.
            var projection = Builders<MongoContentEntity>.Projection.Include("_id");

            foreach (var field in query.GetAllFields())
            {
                projection = projection.Include(field);
            }

            if (query.Random > 0)
            {
                var ids =
                    await collection.Aggregate()
                        .Match(filter)
                        .Project<IdOnly>(projection)
                        .QuerySort(query)
                        .QuerySkip(query)
                        .QueryLimit(query)
                        .ToListAsync(ct);

                var randomIds = ids.Select(x => x.Id).TakeRandom(query.Random);

                var documents =
                    await collection.Find(Builders<MongoContentEntity>.Filter.In(x => x.Id, randomIds))
                        .ToListAsync(ct);

                return documents.Shuffle().ToList();
            }

            var joined =
                await collection.Aggregate()
                    .Match(filter)
                    .Project<IdOnly>(projection)
                    .QuerySort(query)
                    .QuerySkip(query)
                    .QueryLimit(query)
                    .Lookup<IdOnly, MongoContentEntity, MongoContentEntity, MongoContentEntity[], IdOnly>(collection,
                        LookupLet,
                        PipelineDefinitionBuilder.For<MongoContentEntity>()
                            .Match(LookupMatch)
                            .Project(
                                BuildProjection2<MongoContentEntity>(q.Fields)),
                        x => x.Joined)
                    .Project<IdOnly>(
                        Builders<IdOnly>.Projection.Include(x => x.Joined))
                    .ToListAsync(ct);

            return joined.Select(x => x.Joined[0]).ToList();
        }

        var result =
            collection.Find(filter)
                .QuerySort(query)
                .QuerySkip(query)
                .QueryLimit(query)
                .SelectFields(q.Fields)
                .ToListRandomAsync(collection, query.Random, ct);

        return await result;
    }

    public static Task<List<StatusOnly>> FindStatusAsync(this IMongoCollection<MongoContentEntity> collection,
        FilterDefinition<MongoContentEntity> filter,
        CancellationToken ct)
    {
        var projections = Builders<MongoContentEntity>.Projection;

        return collection.Find(filter)
            .Project<StatusOnly>(projections
                .Include(x => x.Id)
                .Include(x => x.IndexedSchemaId)
                .Include(x => x.Status))
            .ToListAsync(ct);
    }

    public static IFindFluent<T, T> SelectFields<T>(this IFindFluent<T, T> find, IEnumerable<string>? fields)
    {
        return find.Project<T>(BuildProjection2<T>(fields));
    }

    public static IAggregateFluent<T> SelectFields<T>(this IAggregateFluent<T> find, IEnumerable<string>? fields)
    {
        return find.Project<T>(BuildProjection2<T>(fields));
    }

    public static ProjectionDefinition<T, T> BuildProjection2<T>(IEnumerable<string>? fields)
    {
        var projector = Builders<T>.Projection;
        var projections = new List<ProjectionDefinition<T>>();

        if (fields?.Any() == true)
        {
            static IEnumerable<string> GetDataFields(IEnumerable<string> fields)
            {
                var dataPrefix = Field.Of<MongoContentEntity>(x => nameof(x.Data));

                foreach (var field in fields)
                {
                    var actualFieldName = field;
                    // Only add data fields, because we add all meta fields anyway.
                    if (FieldNames.IsDataField(field, out var dataField))
                    {
                        actualFieldName = dataField;
                    }

                    var fullName = $"{dataPrefix}.{actualFieldName}";

                    if (!MetaFields.ContainsKey(fullName))
                    {
                        yield return fullName;
                    }
                }
            }

            var addedFields = new List<string>();

            // Sort the fields to start with prefixes first.
            var allFields = GetDataFields(fields).Union(MetaFields.Values).Order();

            foreach (var field in allFields)
            {
                // If there is at least one field that is a prefix of the current field, we cannot add that.
                if (addedFields.Exists(x => field.StartsWith(x, StringComparison.Ordinal)))
                {
                    continue;
                }

                projections.Add(projector.Include(field));

                // Track added prefixes.
                addedFields.Add(field);
            }
        }
        else
        {
            projections.Add(projector.Exclude(Field.Of<MongoContentEntity>(x => nameof(x.NewData))));
        }

        return projector.Combine(projections);
    }
}
