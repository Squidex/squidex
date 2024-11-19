// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO.Pipelines;
using Microsoft.AspNetCore.Http.Features;

namespace Squidex.Web.Pipeline;

internal sealed class UsageResponseBodyFeature(IHttpResponseBodyFeature inner) : IHttpResponseBodyFeature
{
    private readonly UsageStream usageStream = new UsageStream(inner.Stream);
    private readonly UsagePipeWriter usageWriter = new UsagePipeWriter(inner.Writer);
    private long bytesWritten;

    public long BytesWritten
    {
        get => bytesWritten + usageStream.BytesWritten + usageWriter.BytesWritten;
    }

    public Stream Stream
    {
        get => usageStream;
    }

    public PipeWriter Writer
    {
        get => usageWriter;
    }

    public Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        return inner.StartAsync(cancellationToken);
    }

    public Task CompleteAsync()
    {
        return inner.CompleteAsync();
    }

    public void DisableBuffering()
    {
        inner.DisableBuffering();
    }

    public async Task SendFileAsync(string path, long offset, long? count,
        CancellationToken cancellationToken = default)
    {
        await inner.SendFileAsync(path, offset, count, cancellationToken);

        if (count != null)
        {
            bytesWritten += count.Value;
        }
        else
        {
            var file = new FileInfo(path);

            if (file.Exists)
            {
                bytesWritten += file.Length;
            }
        }
    }
}
