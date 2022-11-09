// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NetTopologySuite.Geometries;
using Squidex.Domain.Apps.Entities.Contents.Text;

namespace Squidex.Extensions.Text.ElasticSearch;

public static class CommandFactory
{
    public static void CreateCommands(IndexCommand command, List<object> args, string indexName)
    {
        switch (command)
        {
            case UpsertIndexEntry upsert:
                UpsertEntry(upsert, args, indexName);
                break;
            case UpdateIndexEntry update:
                UpdateEntry(update, args, indexName);
                break;
            case DeleteIndexEntry delete:
                DeleteEntry(delete, args, indexName);
                break;
        }
    }

    private static void UpsertEntry(UpsertIndexEntry upsert, List<object> args, string indexName)
    {
        var geoField = string.Empty;
        var geoObject = (object)null;

        if (upsert.GeoObjects != null)
        {
            foreach (var (key, value) in upsert.GeoObjects)
            {
                if (value is Point point)
                {
                    geoField = key;
                    geoObject = new
                    {
                        lat = point.Coordinate.X,
                        lon = point.Coordinate.Y
                    };
                    break;
                }
            }
        }

        if (upsert.Texts != null || geoObject != null)
        {
            args.Add(new
            {
                index = new
                {
                    _id = upsert.DocId,
                    _index = indexName
                }
            });

            var texts = new Dictionary<string, string>();

            foreach (var (key, value) in upsert.Texts)
            {
                var text = value;

                var languageCode = ElasticSearchIndexDefinition.GetFieldName(key);

                if (texts.TryGetValue(languageCode, out var existing))
                {
                    text = $"{existing} {value}";
                }

                texts[languageCode] = text;
            }

            args.Add(new
            {
                appId = upsert.AppId.Id.ToString(),
                appName = upsert.AppId.Name,
                contentId = upsert.ContentId.ToString(),
                schemaId = upsert.SchemaId.Id.ToString(),
                schemaName = upsert.SchemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts,
                geoField,
                geoObject
            });
        }
    }

    private static void UpdateEntry(UpdateIndexEntry update, List<object> args, string indexName)
    {
        args.Add(new
        {
            update = new
            {
                _id = update.DocId,
                _index = indexName
            }
        });

        args.Add(new
        {
            doc = new
            {
                serveAll = update.ServeAll,
                servePublished = update.ServePublished
            }
        });
    }

    private static void DeleteEntry(DeleteIndexEntry delete, List<object> args, string indexName)
    {
        args.Add(new
        {
            delete = new
            {
                _id = delete.DocId,
                _index = indexName
            }
        });
    }
}
