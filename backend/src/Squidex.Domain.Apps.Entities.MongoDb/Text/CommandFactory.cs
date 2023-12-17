// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class CommandFactory<T> : MongoBase<MongoTextIndexEntity<T>> where T : class
{
    private readonly Func<Dictionary<string, string>, T> textBuilder;

    public CommandFactory(Func<Dictionary<string, string>, T> textBuilder)
    {
        this.textBuilder = textBuilder;
    }

    public void CreateCommands(IndexCommand command, List<WriteModel<MongoTextIndexEntity<T>>> writes)
    {
        switch (command)
        {
            case DeleteIndexEntry delete:
                DeleteEntry(delete, writes);
                break;
            case UpsertIndexEntry upsert:
                UpsertEntry(upsert, writes);
                break;
            case UpdateIndexEntry update:
                UpdateEntry(update, writes);
                break;
        }
    }

    private void UpsertEntry(UpsertIndexEntry upsert, List<WriteModel<MongoTextIndexEntity<T>>> writes)
    {
        writes.Add(
            new UpdateOneModel<MongoTextIndexEntity<T>>(
                Filter.And(
                    FilterByCommand(upsert),
                    Filter.Exists(x => x.GeoField, false),
                    Filter.Exists(x => x.GeoObject, false)),
                Update
                    .Set(x => x.ServeAll, upsert.ServeAll)
                    .Set(x => x.ServePublished, upsert.ServePublished)
                    .Set(x => x.Texts, BuildTexts(upsert))
                    .SetOnInsert(x => x.Id, ObjectId.GenerateNewId())
                    .SetOnInsert(x => x.AppId, upsert.UniqueContentId.AppId)
                    .SetOnInsert(x => x.ContentId, upsert.UniqueContentId.ContentId)
                    .SetOnInsert(x => x.Stage, upsert.Stage)
                    .SetOnInsert(x => x.SchemaId, upsert.SchemaId.Id))
            {
                IsUpsert = true
            });

        if (upsert.GeoObjects?.Count > 0)
        {
            if (!upsert.IsNew)
            {
                writes.Add(
                    new DeleteManyModel<MongoTextIndexEntity<T>>(
                        Filter.And(
                            FilterByCommand(upsert),
                            Filter.Exists(x => x.GeoField),
                            Filter.Exists(x => x.GeoObject))));
            }

            foreach (var (field, geoObject) in upsert.GeoObjects)
            {
                writes.Add(
                    new InsertOneModel<MongoTextIndexEntity<T>>(
                        new MongoTextIndexEntity<T>
                        {
                            Id = ObjectId.GenerateNewId(),
                            AppId = upsert.UniqueContentId.AppId,
                            ContentId = upsert.UniqueContentId.ContentId,
                            GeoField = field,
                            GeoObject = geoObject,
                            SchemaId = upsert.SchemaId.Id,
                            ServeAll = upsert.ServeAll,
                            ServePublished = upsert.ServePublished,
                            Stage = upsert.Stage,
                        }));
            }
        }
    }

    private T? BuildTexts(UpsertIndexEntry upsert)
    {
        return upsert.Texts == null ? null : textBuilder(upsert.Texts);
    }

    private static void UpdateEntry(UpdateIndexEntry update, List<WriteModel<MongoTextIndexEntity<T>>> writes)
    {
        writes.Add(
            new UpdateOneModel<MongoTextIndexEntity<T>>(
                FilterByCommand(update),
                Update
                    .Set(x => x.ServeAll, update.ServeAll)
                    .Set(x => x.ServePublished, update.ServePublished)));
    }

    private static void DeleteEntry(DeleteIndexEntry delete, List<WriteModel<MongoTextIndexEntity<T>>> writes)
    {
        writes.Add(
            new DeleteManyModel<MongoTextIndexEntity<T>>(
                FilterByCommand(delete)));
    }

    private static FilterDefinition<MongoTextIndexEntity<T>> FilterByCommand(IndexCommand command)
    {
        return Filter.And(
            Filter.Eq(x => x.AppId, command.UniqueContentId.AppId),
            Filter.Eq(x => x.ContentId, command.UniqueContentId.ContentId),
            Filter.Eq(x => x.Stage, command.Stage));
    }
}
