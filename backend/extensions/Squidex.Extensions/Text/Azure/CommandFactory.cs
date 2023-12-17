// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Azure.Search.Documents.Models;
using NetTopologySuite.Geometries;
using Squidex.Domain.Apps.Entities.Contents.Text;

namespace Squidex.Extensions.Text.Azure;

public static class CommandFactory
{
    public static void CreateCommands(IndexCommand command, IList<IndexDocumentsAction<SearchDocument>> batch)
    {
        switch (command)
        {
            case UpsertIndexEntry upsert:
                UpsertTextEntry(upsert, batch);
                break;
            case UpdateIndexEntry update:
                UpdateEntry(update, batch);
                break;
            case DeleteIndexEntry delete:
                DeleteEntry(delete, batch);
                break;
        }
    }

    private static void UpsertTextEntry(UpsertIndexEntry upsert, IList<IndexDocumentsAction<SearchDocument>> batch)
    {
        var geoField = string.Empty;
        var geoObject = (object?)null;

        if (upsert.GeoObjects != null)
        {
            foreach (var (key, value) in upsert.GeoObjects)
            {
                if (value is Point point)
                {
                    geoField = key;
                    geoObject = new
                    {
                        type = "Point",
                        coordinates = new[]
                        {
                            point.Coordinate.X,
                            point.Coordinate.Y
                        }
                    };
                    break;
                }
            }
        }

        if (upsert.Texts != null && geoObject != null)
        {
            var document = new SearchDocument
            {
                ["docId"] = upsert.ToDocId(),
                ["appId"] = upsert.UniqueContentId.AppId.ToString(),
                ["appName"] = string.Empty,
                ["contentId"] = upsert.UniqueContentId.ToString(),
                ["schemaId"] = upsert.SchemaId.Id.ToString(),
                ["schemaName"] = upsert.SchemaId.Name,
                ["serveAll"] = upsert.ServeAll,
                ["servePublished"] = upsert.ServePublished,
                ["geoField"] = geoField,
                ["geoObject"] = geoObject
            };

            foreach (var (key, value) in upsert.Texts)
            {
                var text = value;

                var languageCode = AzureIndexDefinition.GetFieldName(key);

                if (document.TryGetValue(languageCode, out var existing))
                {
                    text = $"{existing} {value}";
                }

                document[languageCode] = text;
            }

            batch.Add(IndexDocumentsAction.MergeOrUpload(document));
        }
    }

    private static void UpdateEntry(UpdateIndexEntry update, IList<IndexDocumentsAction<SearchDocument>> batch)
    {
        var document = new SearchDocument
        {
            ["docId"] = update.ToDocId(),
            ["serveAll"] = update.ServeAll,
            ["servePublished"] = update.ServePublished
        };

        batch.Add(IndexDocumentsAction.MergeOrUpload(document));
    }

    private static void DeleteEntry(DeleteIndexEntry delete, IList<IndexDocumentsAction<SearchDocument>> batch)
    {
        batch.Add(IndexDocumentsAction.Delete("docId", delete.ToDocId().ToBase64()));
    }

    private static string ToBase64(this string value)
    {
        return Convert.ToBase64String(Encoding.Default.GetBytes(value));
    }
}
