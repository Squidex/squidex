// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Elastic
{
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
            args.Add(new
            {
                index = new
                {
                    _id = upsert.DocId,
                    _index = indexName
                }
            });

            args.Add(new
            {
                appId = upsert.AppId.Id.ToString(),
                appName = upsert.AppId.Name,
                contentId = upsert.ContentId.ToString(),
                schemaId = upsert.SchemaId.Id.ToString(),
                schemaName = upsert.SchemaId.Name,
                serveAll = upsert.ServeAll,
                servePublished = upsert.ServePublished,
                texts = upsert.Texts
            });
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
}
