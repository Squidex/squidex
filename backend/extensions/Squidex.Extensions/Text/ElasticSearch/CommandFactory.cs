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
        var hasAddedDeletion = false;

        void AddArgs(object arg)
        {
            if (!hasAddedDeletion)
            {
                args.Add(new
                {
                    index = new
                    {
                        _id = upsert.ToDocId(),
                        _index = indexName,
                    },
                });
            }

            args.Add(arg);
            hasAddedDeletion = true;
        }

        if (upsert.GeoObjects != null)
        {
            foreach (var (key, value) in upsert.GeoObjects)
            {
                if (value is not Point point)
                {
                    continue;
                }

                var geoField = key;
                var geoObject = new
                {
                    lat = point.Coordinate.X,
                    lon = point.Coordinate.Y,
                };

                AddArgs(new
                {
                    appId = upsert.UniqueContentId.AppId.ToString(),
                    appName = string.Empty,
                    contentId = upsert.UniqueContentId.ContentId.ToString(),
                    schemaId = upsert.SchemaId.Id.ToString(),
                    schemaName = upsert.SchemaId.Name,
                    serveAll = upsert.ServeAll,
                    servePublished = upsert.ServePublished,
                    geoField,
                    geoObject,
                });
            }
        }

        if (upsert.UserInfos != null)
        {
            foreach (var userInfo in upsert.UserInfos)
            {
                var userInfoApiKey = userInfo.ApiKey;
                var userInfoRole = userInfo.Role;

                AddArgs(new
                {
                    appId = upsert.UniqueContentId.AppId.ToString(),
                    appName = string.Empty,
                    contentId = upsert.UniqueContentId.ContentId.ToString(),
                    schemaId = upsert.SchemaId.Id.ToString(),
                    schemaName = upsert.SchemaId.Name,
                    serveAll = upsert.ServeAll,
                    servePublished = upsert.ServePublished,
                    userInfoApiKey,
                    userInfoRole,
                });
            }
        }

        if (upsert.Texts is { Count: > 0 })
        {
            var texts = new Dictionary<string, string>();

            foreach (var (key, value) in upsert.Texts)
            {
                var textMerged = value;
                var textLanguage = ElasticSearchIndexDefinition.GetFieldName(key);

                if (texts.TryGetValue(textLanguage, out var existing))
                {
                    textMerged = $"{existing} {value}";
                }

                if (!string.IsNullOrWhiteSpace(textMerged))
                {
                    texts[textLanguage] = textMerged;
                }
            }

            AddArgs(new
            {
                appId = upsert.UniqueContentId.AppId.ToString(),
                appName = string.Empty,
                contentId = upsert.UniqueContentId.ContentId.ToString(),
                schemaId = upsert.SchemaId.Id.ToString(),
                schemaName = upsert.SchemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts,
            });
        }
    }

    private static void UpdateEntry(UpdateIndexEntry update, List<object> args, string indexName)
    {
        args.Add(new
        {
            update = new
            {
                _id = update.ToDocId(),
                _index = indexName,
            },
        });

        args.Add(new
        {
            doc = new
            {
                serveAll = update.ServeAll,
                servePublished = update.ServePublished,
            },
        });
    }

    private static void DeleteEntry(DeleteIndexEntry delete, List<object> args, string indexName)
    {
        args.Add(new
        {
            delete = new
            {
                _id = delete.ToDocId(),
                _index = indexName,
            },
        });
    }
}
