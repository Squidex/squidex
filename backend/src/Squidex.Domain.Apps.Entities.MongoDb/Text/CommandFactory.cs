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
                    Filter.Eq(x => x.DocId, upsert.DocId),
                    Filter.Exists(x => x.GeoField, false),
                    Filter.Exists(x => x.GeoObject, false)),
                Update
                    .Set(x => x.ScopeAll, upsert.ScopeAll)
                    .Set(x => x.ScopePublished, upsert.ScopePublished)
                    .Set(x => x.Texts, BuildTexts(upsert))
                    .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                    .SetOnInsert(x => x.DocId, upsert.DocId)
                    .SetOnInsert(x => x.AppId, upsert.AppId.Id)
                    .SetOnInsert(x => x.ContentId, upsert.ContentId)
                    .SetOnInsert(x => x.SchemaId, upsert.SchemaId.Id))
            {
                IsUpsert = true
            });

        if (upsert.GeoObjects?.Count > 0)
        {
            if (!upsert.IsNew)
            {
                writes.Add(
                    new DeleteOneModel<MongoTextIndexEntity<T>>(
                        Filter.And(
                            Filter.Eq(x => x.DocId, upsert.DocId),
                            Filter.Exists(x => x.GeoField),
                            Filter.Exists(x => x.GeoObject))));
            }

            foreach (var (field, geoObject) in upsert.GeoObjects)
            {
                writes.Add(
                    new InsertOneModel<MongoTextIndexEntity<T>>(
                        new MongoTextIndexEntity<T>
                        {
                            DocId = upsert.DocId,
                            AppId = upsert.AppId.Id,
                            ContentId = upsert.ContentId,
                            GeoField = field,
                            GeoObject = geoObject,
                            SchemaId = upsert.SchemaId.Id,
                            ScopeAll = upsert.ScopeAll,
                            ScopePublished = upsert.ScopePublished,
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
                Filter.Eq(x => x.DocId, update.DocId),
                Update
                    .Set(x => x.ScopeAll, update.ServeAll)
                    .Set(x => x.ScopePublished, update.ServePublished)));
    }

    private static void DeleteEntry(DeleteIndexEntry delete, List<WriteModel<MongoTextIndexEntity<T>>> writes)
    {
        writes.Add(
            new DeleteOneModel<MongoTextIndexEntity<T>>(
                Filter.Eq(x => x.DocId, delete.DocId)));
    }
}
