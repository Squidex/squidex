// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

internal sealed class TextIndexCommands(IJsonSerializer serializer)
{
    private readonly Dictionary<(UniqueContentId, byte), IndexCommand> commands = [];

    // The values are only calculated for the final commands, because older versions are replaced anyway.
    private readonly Dictionary<UpsertIndexEntry, TextIndexSource> sources = [];

    public async Task WriteAsync(ITextIndex textIndex, TextIndexExtraction extraction,
        CancellationToken ct)
    {
        if (commands.Count == 0)
        {
            return;
        }

        var texts = await extraction.ExtractAsync(sources.Values, ct);

        foreach (var (upsert, source) in sources)
        {
            upsert.GeoObjects = source.Data.ToGeo(serializer);
            upsert.Texts = texts.GetValueOrDefault(source.Data);

            if (source.IncludeUserInfos)
            {
                upsert.UserInfos = source.Data.ToUserInfos();
            }
        }

        await textIndex.ExecuteAsync(commands.Values.ToArray(), ct);

        commands.Clear();
        sources.Clear();
    }

    public void Create(ContentEvent @event, UniqueContentId uniqueId, ContentData data)
    {
        var upsert = new UpsertIndexEntry
        {
            UniqueContentId = uniqueId,
            IsNew = true,
            Stage = 0,
            ServeAll = true,
            ServePublished = false,
        };

        Index(@event, upsert, new TextIndexSource(@event, data, true));
    }

    public void Upsert(ContentEvent @event, UniqueContentId uniqueId, byte stage, bool all, bool published, ContentData data)
    {
        var upsert = new UpsertIndexEntry
        {
            UniqueContentId = uniqueId,
            Stage = stage,
            ServeAll = all,
            ServePublished = published,
        };

        Index(@event, upsert, new TextIndexSource(@event, data, false));
    }

    public void Update(ContentEvent @event, UniqueContentId uniqueId, byte stage, bool all, bool published)
    {
        Index(@event,
            new UpdateIndexEntry
            {
                UniqueContentId = uniqueId,
                Stage = stage,
                ServeAll = all,
                ServePublished = published,
            });
    }

    public void Delete(ContentEvent @event, UniqueContentId uniqueId, byte stage)
    {
        Index(@event,
            new DeleteIndexEntry
            {
                UniqueContentId = uniqueId,
                Stage = stage,
            });
    }

    public void PrepareRebuild()
    {
        var contents = new Dictionary<UniqueContentId, NamedId<DomainId>>();

        foreach (var command in commands.Values)
        {
            // The entries already exist, therefore old geo objects and user infos must be deleted.
            if (command is UpsertIndexEntry upsert)
            {
                upsert.IsNew = false;
            }

            contents[command.UniqueContentId] = command.SchemaId;
        }

        // Delete entries of stages that do not exist anymore.
        foreach (var (uniqueId, schemaId) in contents)
        {
            for (byte stage = 0; stage < 2; stage++)
            {
                if (!commands.ContainsKey((uniqueId, stage)))
                {
                    commands[(uniqueId, stage)] = new DeleteIndexEntry { UniqueContentId = uniqueId, SchemaId = schemaId, Stage = stage };
                }
            }
        }
    }

    private void Index(ContentEvent @event, IndexCommand command, TextIndexSource? source = null)
    {
        command.SchemaId = @event.SchemaId;

        var key = (command.UniqueContentId, command.Stage);

        // Merge the flags into a pending upsert, because it would otherwise be replaced.
        if (command is UpdateIndexEntry update && commands.TryGetValue(key, out var existing) && existing is UpsertIndexEntry pending)
        {
            pending.ServeAll = update.ServeAll;
            pending.ServePublished = update.ServePublished;
            return;
        }

        // Release the data of the replaced version.
        if (commands.TryGetValue(key, out var replaced) && replaced is UpsertIndexEntry replacedUpsert)
        {
            sources.Remove(replacedUpsert);
        }

        commands[key] = command;

        if (command is UpsertIndexEntry upsert && source != null)
        {
            sources[upsert] = source;
        }
    }
}
