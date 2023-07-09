// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure.Json;

namespace Squidex.Web.Pipeline;

public sealed class JsonStreamResult<T> : ActionResult
{
#pragma warning disable RECS0108 // Warns about static fields in generic types
    public static readonly byte[] Prefix = Encoding.UTF8.GetBytes("data: ");
    public static readonly byte[] Separator = Encoding.UTF8.GetBytes("\n\n");
#pragma warning restore RECS0108 // Warns about static fields in generic types
    private readonly IAsyncEnumerable<T> stream;

    public JsonStreamResult(IAsyncEnumerable<T> stream)
    {
        this.stream = stream;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        DisableResponseBuffering(context.HttpContext);

        var serializer = context.HttpContext.RequestServices.GetRequiredService<IJsonSerializer>();

        // The official content type for server sent events.
        context.HttpContext.Request.Headers[HeaderNames.ContentType] = "text/event-stream";
        context.HttpContext.Request.Headers[HeaderNames.CacheControl] = "no-cache";

        var ct = context.HttpContext.RequestAborted;

        var body = context.HttpContext.Response.Body;

        await foreach (var item in stream.WithCancellation(context.HttpContext.RequestAborted))
        {
            // Every line needs to start with data.
            await body.WriteAsync(Prefix, ct);

            await serializer.SerializeAsync(item, body, false, ct);

            // Write the separator after a every json object to simplify deserialization.
            await body.WriteAsync(Separator, ct);
        }
    }

    private static void DisableResponseBuffering(HttpContext context)
    {
        var bufferingFeature = context.Features.Get<IHttpResponseBodyFeature>();

        bufferingFeature?.DisableBuffering();
    }
}
