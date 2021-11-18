// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Azure.Search.Documents.Models;
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
                    batch.Add(UpsertEntry(upsert));
                    break;
                case UpdateIndexEntry update:
                    batch.Add(UpdateEntry(update));
                    break;
                case DeleteIndexEntry delete:
                    batch.Add(DeleteEntry(delete));
                    break;
            }
        }

        private static IndexDocumentsAction<SearchDocument> UpsertEntry(UpsertIndexEntry upsert)
        {
            var searchDocument = new SearchDocument
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

            if (upsert.Texts != null)
            {
                foreach (var (key, value) in upsert.Texts)
                {
                    searchDocument[AzureIndexDefinition.GetTextField(key)] = value;
                }
            }

            return IndexDocumentsAction.MergeOrUpload(searchDocument);
        }

        private static IndexDocumentsAction<SearchDocument> UpdateEntry(UpdateIndexEntry update)
        {
            var searchDocument = new SearchDocument
            {
                ["docId"] = update.DocId.ToBase64(),
                ["serveAll"] = update.ServeAll,
                ["servePublished"] = update.ServePublished,
            };

            return IndexDocumentsAction.MergeOrUpload(searchDocument);
        }

        private static IndexDocumentsAction<SearchDocument> DeleteEntry(DeleteIndexEntry delete)
        {
            return IndexDocumentsAction.Delete("docId", delete.DocId.ToBase64());
        }

        private static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(value));
        }
    }
}
