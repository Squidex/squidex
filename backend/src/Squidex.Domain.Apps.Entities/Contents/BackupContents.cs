// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class BackupContents : IBackupHandler
{
    private const int BatchSize = 100;
    private const string UrlsFile = "Urls.json";
    private readonly Dictionary<DomainId, HashSet<DomainId>> contentIdsBySchemaId = new Dictionary<DomainId, HashSet<DomainId>>();
    private readonly Rebuilder rebuilder;
    private readonly IUrlGenerator urlGenerator;
    private Urls? assetsUrlNew;
    private Urls? assetsUrlOld;

    public string Name { get; } = "Contents";

    public sealed class Urls
    {
        public string Assets { get; set; }

        public string AssetsApp { get; set; }
    }

    public BackupContents(Rebuilder rebuilder, IUrlGenerator urlGenerator)
    {
        this.rebuilder = rebuilder;
        this.urlGenerator = urlGenerator;
    }

    public async Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context,
        CancellationToken ct)
    {
        if (@event.Payload is AppCreated appCreated)
        {
            var urls = GetUrls(appCreated.Name);

            await context.Writer.WriteJsonAsync(UrlsFile, urls, ct);
        }
    }

    public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context,
        CancellationToken ct)
    {
        switch (@event.Payload)
        {
            case AppCreated appCreated:
                assetsUrlNew = GetUrls(appCreated.Name);
                assetsUrlOld = await ReadUrlsAsync(context.Reader, ct);
                break;
            case SchemaDeleted schemaDeleted:
                contentIdsBySchemaId.Remove(schemaDeleted.SchemaId.Id);
                break;
            case ContentCreated contentCreated:
                contentIdsBySchemaId.GetOrAddNew(contentCreated.SchemaId.Id)
                    .Add(@event.Headers.AggregateId());

                if (assetsUrlNew != null && assetsUrlOld != null)
                {
                    ReplaceAssetUrl(contentCreated.Data);
                }

                break;
            case ContentUpdated contentUpdated:
                if (assetsUrlNew != null && assetsUrlOld != null)
                {
                    ReplaceAssetUrl(contentUpdated.Data);
                }

                break;
        }

        return true;
    }

    private void ReplaceAssetUrl(ContentData data)
    {
        foreach (var field in data.Values)
        {
            if (field != null)
            {
                ReplaceAssetUrl(field);
            }
        }
    }

    private void ReplaceAssetUrl(IDictionary<string, JsonValue> source)
    {
        List<(string, string)>? replacements = null;

        foreach (var (key, value) in source)
        {
            switch (value.Value)
            {
                case string s:
                    {
                        var newValue = s.Replace(assetsUrlOld!.AssetsApp, assetsUrlNew!.AssetsApp, StringComparison.Ordinal);

                        if (!ReferenceEquals(newValue, s))
                        {
                            replacements ??= new List<(string, string)>();
                            replacements.Add((key, newValue));
                            break;
                        }

                        newValue = newValue.Replace(assetsUrlOld!.Assets, assetsUrlNew!.Assets, StringComparison.Ordinal);

                        if (!ReferenceEquals(newValue, s))
                        {
                            replacements ??= new List<(string, string)>();
                            replacements.Add((key, newValue));
                            break;
                        }
                    }

                    break;

                case JsonArray a:
                    ReplaceAssetUrl(a);
                    break;

                case JsonObject o:
                    ReplaceAssetUrl(o);
                    break;
            }
        }

        if (replacements != null)
        {
            foreach (var (key, newValue) in replacements)
            {
                source[key] = newValue;
            }
        }
    }

    private void ReplaceAssetUrl(List<JsonValue> source)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var value = source[i];

            switch (value.Value)
            {
                case string s:
                    {
                        var newValue = s.Replace(assetsUrlOld!.AssetsApp, assetsUrlNew!.AssetsApp, StringComparison.Ordinal);

                        if (!ReferenceEquals(newValue, s))
                        {
                            source[i] = newValue;
                            break;
                        }

                        newValue = newValue.Replace(assetsUrlOld!.Assets, assetsUrlNew!.Assets, StringComparison.Ordinal);

                        if (!ReferenceEquals(newValue, s))
                        {
                            source[i] = newValue;
                            break;
                        }
                    }

                    break;

                case JsonObject o:
                    ReplaceAssetUrl(o);
                    break;
            }
        }
    }

    public async Task RestoreAsync(RestoreContext context,
        CancellationToken ct)
    {
        var ids = contentIdsBySchemaId.Values.SelectMany(x => x);

        if (ids.Any())
        {
            await rebuilder.InsertManyAsync<ContentDomainObject, ContentDomainObject.State>(ids, BatchSize, ct);
        }
    }

    private static async Task<Urls?> ReadUrlsAsync(IBackupReader reader,
        CancellationToken ct)
    {
        try
        {
            return await reader.ReadJsonAsync<Urls>(UrlsFile, ct);
        }
        catch
        {
            return null;
        }
    }

    private Urls GetUrls(string appName)
    {
        return new Urls
        {
            Assets = urlGenerator.AssetContentBase(),
            AssetsApp = urlGenerator.AssetContentBase(appName)
        };
    }
}
