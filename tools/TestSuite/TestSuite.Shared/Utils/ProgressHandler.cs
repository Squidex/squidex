// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite.Utils;

public sealed class ProgressHandler : IAssetProgressHandler
{
    public string FileId { get; private set; } = Guid.NewGuid().ToString();

    public List<int> Progress { get; } = [];

    public List<int> Uploads { get; } = [];

    public Exception Exception { get; private set; }

    public AssetDto Asset { get; private set; }

    public AssetUploadOptions AsOptions(string? id = null)
    {
        var options = default(AssetUploadOptions);
        options.ProgressHandler = this;
        options.FileId = FileId;
        options.Id = id;

        return options;
    }

    public void Uploaded()
    {
        Uploads.Add(Progress.LastOrDefault());
    }

    public Task OnCompletedAsync(AssetUploadCompletedEvent @event,
        CancellationToken ct)
    {
        Asset = @event.Asset;
        return Task.CompletedTask;
    }

    public Task OnCreatedAsync(AssetUploadCreatedEvent @event,
        CancellationToken ct)
    {
        FileId = @event.FileId;
        return Task.CompletedTask;
    }

    public Task OnProgressAsync(AssetUploadProgressEvent @event,
        CancellationToken ct)
    {
        Progress.Add(@event.Progress);
        return Task.CompletedTask;
    }

    public Task OnFailedAsync(AssetUploadExceptionEvent @event,
        CancellationToken ct)
    {
        Exception = @event.Exception;
        return Task.CompletedTask;
    }
}
