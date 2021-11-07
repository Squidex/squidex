// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Azure.Search.Documents.Models;
using GeoJSON.Net.Geometry;
using Squidex.Domain.Apps.Entities.Contents.Text;

namespace Squidex.Extensions.Text.Azure
{
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
            static SearchDocument CreateDoc(UpsertIndexEntry upsert)
            {
                return new SearchDocument
                {
                    ["docId"] = upsert.DocId.ToBase64(),
                    ["appId"] = upsert.AppId.Id.ToString(),
                    ["appName"] = upsert.AppId.Name,
                    ["contentId"] = upsert.ContentId.ToString(),
                    ["schemaId"] = upsert.SchemaId.Id.ToString(),
                    ["schemaName"] = upsert.SchemaId.Name,
                    ["serveAll"] = upsert.ServeAll,
                    ["servePublished"] = upsert.ServePublished
                };
            }

            if (upsert.Texts != null)
            {
                var searchDocument = CreateDoc(upsert);

                foreach (var (key, value) in upsert.Texts)
                {
                    searchDocument[AzureIndexDefinition.GetTextField(key)] = value;
                }

                batch.Add(IndexDocumentsAction.MergeOrUpload(searchDocument));
            }
            else if (upsert.GeoObjects != null)
            {
                foreach (var (key, value) in upsert.GeoObjects)
                {
                    var geography = value.ToSpatialGeometry();

                    if (geography != null)
                    {
                        var searchDocument = CreateDoc(upsert);

                        searchDocument["geoField"] = key;
                        searchDocument["geoObject"] = new
                        {
                            type = "Point",
                            coordinates = new[] { geography.Longitude, geography.Latitude }
                        };

                        batch.Add(IndexDocumentsAction.MergeOrUpload(searchDocument));
                        break;
                    }
                }
            }

        }

        private static void UpdateEntry(UpdateIndexEntry update, IList<IndexDocumentsAction<SearchDocument>> batch)
        {
            var searchDocument = new SearchDocument
            {
                ["docId"] = update.DocId.ToBase64(),
                ["serveAll"] = update.ServeAll,
                ["servePublished"] = update.ServePublished,
            };

            batch.Add(IndexDocumentsAction.MergeOrUpload(searchDocument));
        }

        private static void DeleteEntry(DeleteIndexEntry delete, IList<IndexDocumentsAction<SearchDocument>> batch)
        {
            batch.Add(IndexDocumentsAction.Delete("docId", delete.DocId.ToBase64()));
        }

        private static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(value));
        }
    }
}
