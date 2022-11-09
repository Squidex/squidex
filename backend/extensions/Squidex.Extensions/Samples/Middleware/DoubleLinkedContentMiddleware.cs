// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Extensions.Samples.Middleware;

public sealed class DoubleLinkedContentMiddleware : ICustomCommandMiddleware
{
    private readonly IContentLoader contentLoader;

    public DoubleLinkedContentMiddleware(IContentLoader contentLoader)
    {
        this.contentLoader = contentLoader;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.Command is UpdateContent update && context.IsCompleted && update.SchemaId.Name == "source")
        {
            // After a change is made, the content is put to the command context.
            var content = context.Result<IContentEntity>();

            var contentPrevious =
                await contentLoader.GetAsync(
                    content.AppId.Id,
                    content.Id,
                    content.Version - 1,
                    ct);

            // The data might have been changed within the domain object. Therefore we do not use the data fro mthe command.
            var oldReferenceId = GetReference(contentPrevious?.Data);
            var newReferenceId = GetReference(content.Data);

            // If nothing has been changed we can just stop here.
            if (newReferenceId == oldReferenceId)
            {
                return;
            }

            if (oldReferenceId != null)
            {
                var oldReferenced = await contentLoader.GetAsync(content.AppId.Id, DomainId.Create(oldReferenceId), ct: ct);

                if (oldReferenced != null)
                {
                    var data = oldReferenced.Data.Clone();

                    // Remove the reference from the old referenced content.
                    data.Remove("referencing");

                    await UpdateReferencing(context, oldReferenced, data, ct);
                }
            }

            if (newReferenceId != null)
            {
                var newReferenced = await contentLoader.GetAsync(content.AppId.Id, DomainId.Create(newReferenceId), ct: ct);

                if (newReferenced != null)
                {
                    var data = newReferenced.Data.Clone();

                    // Add the reference to the new referenced content.
                    data["referencing"] = new ContentFieldData
                    {
                        ["iv"] = JsonValue.Array(content.Id)
                    };

                    await UpdateReferencing(context, newReferenced, data, ct);
                }
            }
        }
    }

    private static async Task UpdateReferencing(CommandContext context, IContentEntity reference, ContentData data,
        CancellationToken ct)
    {
        // Also set the expected version, otherwise it will be overriden with the version from the request.
        await context.CommandBus.PublishAsync(new UpdateContent
        {
            AppId = reference.AppId,
            SchemaId = reference.SchemaId,
            ContentId = reference.Id,
            DoNotScript = true,
            DoNotValidate = true,
            Data = data,
            ExpectedVersion = reference.Version
        }, ct);
    }

    private static string GetReference(ContentData data)
    {
        if (data != null && data.TryGetValue("reference", out ContentFieldData fieldData))
        {
            return fieldData.Values.OfType<JsonArray>().SelectMany(x => x).SingleOrDefault().ToString();
        }

        return null;
    }
}
