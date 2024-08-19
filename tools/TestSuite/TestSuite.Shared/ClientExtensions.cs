// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Utils;

namespace TestSuite;

public static class ClientExtensions
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private static TimeSpan GetTimeout(TimeSpan timeout)
    {
        if (timeout == default)
        {
            return DefaultTimeout;
        }

        return timeout;
    }

    public static bool IsPost(this WebhookRequest request)
    {
        return request.Method == "POST";
    }

    public static bool HasContent(this WebhookRequest request, string content)
    {
        return request.Content.Contains(content, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<bool> PollForDeletionAsync(this IAssetsClient client, string id,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await client.GetAssetAsync(id, cts.Token);
                }
                catch (SquidexException ex) when (ex.StatusCode == 404)
                {
                    return true;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return false;
    }

    public static async Task<AssetDto?> PollAsync(this IAssetsClient client, Func<AssetDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetAssetsAsync(null, cts.Token);
                var result = results.Items.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<ContentsResult<TEntity, TData>> PollAsync<TEntity, TData>(this IContentsClient<TEntity, TData> client, ContentQuery q, Func<TEntity, bool> predicate,
        TimeSpan timeout = default) where TEntity : Content<TData> where TData : class, new()
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var result = await client.GetAsync(q, null, cts.Token);

                if (result.Items.Any(predicate))
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return new ContentsResult<TEntity, TData>();
    }

    public static async Task<SearchResultDto?> PollAsync(this ISearchClient client, string query, Func<SearchResultDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetSearchResultsAsync(query, cts.Token);
                var result = results.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<WebhookRequest?> PollAsync(this WebhookCatcherClient client, string sessionId, Func<WebhookRequest, bool> predicate,
        TimeSpan timeout = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromMinutes(2);
        }

        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetRequestsAsync(sessionId, cts.Token);
                var result = results.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(50, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<HistoryEventDto?> PollAsync(this IHistoryClient client, string? channel, Func<HistoryEventDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetAppHistoryAsync(channel, cts.Token);
                var result = results.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    [Obsolete("Replaced with jobs.")]
    public static async Task<BackupJobDto?> PollAsync(this IBackupsClient client, Func<BackupJobDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetBackupsAsync(cts.Token);
                var result = results.Items.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<JobDto?> PollAsync(this IJobsClient client, Func<JobDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var results = await client.GetJobsAsync(cts.Token);
                var result = results.Items.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<IDictionary<string, int>> PollTagAsync(this IAssetsClient client, string id,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var result = await client.GetTagsAsync(cts.Token);

                if (result.TryGetValue(id, out var count) && count > 0)
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return await client.GetTagsAsync();
    }

    public static async Task<RestoreJobDto?> PollRestoreAsync(this IBackupsClient client, Func<RestoreJobDto, bool> predicate,
        TimeSpan timeout = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout(timeout));

            while (!cts.IsCancellationRequested)
            {
                var result = await client.GetRestoreJobAsync(cts.Token);

                if (predicate(result))
                {
                    return result;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<MemoryStream?> DownloadAsync(this ClientFixture fixture, AssetDto asset, int? version = null)
    {
        var temp = new MemoryStream();

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(fixture.Url);

            var url = asset.Links["content"].Href[1..];

            if (version > 0)
            {
                url += $"?version={version}";
            }

            using (var response = await httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();

                await using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(temp);
                }
            }
        }

        return temp;
    }

    public static async Task UploadInChunksAsync(this IAssetsClient client, ProgressHandler progress, FileParameter fileParameter, string? id = null)
    {
        var pausingStream = new PauseStream(fileParameter.Data, 0.25);
        var pausingFile = new FileParameter(pausingStream, fileParameter.FileName, fileParameter.ContentType)
        {
            ContentLength = fileParameter.Data.Length
        };

        await using (pausingFile.Data)
        {
            using var cts = new CancellationTokenSource(5000);

            while (progress.Asset == null && progress.Exception == null && !cts.IsCancellationRequested)
            {
                pausingStream.Reset();

                await client.UploadAssetAsync(pausingFile, progress.AsOptions(id), cts.Token);
                progress.Uploaded();
            }
        }
    }

    public static async Task<AssetDto> UploadFileAsync(this IAssetsClient client, string path, AssetDto asset, string? fileName = null)
    {
        var fileInfo = new FileInfo(path);

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileName ?? fileInfo.Name, asset.MimeType);

            return await client.PutAssetContentAsync(asset.Id, upload);
        }
    }

    public static async Task<AssetDto> UploadFileAsync(this IAssetsClient client, string path, string fileType, string? fileName = null, string? parentId = null, string? id = null)
    {
        var fileInfo = new FileInfo(path);

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileName ?? fileInfo.Name, fileType);

            return await client.PostAssetAsync(parentId, id, true, upload);
        }
    }

    public static async Task<AssetDto> ReplaceFileAsync(this IAssetsClient client, string id, string path, string fileType, string? fileName = null)
    {
        var fileInfo = new FileInfo(path);

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileName ?? fileInfo.Name, fileType);

            return await client.PutAssetContentAsync(id, upload);
        }
    }

    public static async Task<AssetDto> UploadRandomFileAsync(this IAssetsClient client, int size, string? parentId = null, string? id = null)
    {
        using (var stream = RandomAsset(size))
        {
            var upload = new FileParameter(stream, RandomName(".txt"), "text/csv");

            return await client.PostAssetAsync(parentId, id, true, upload);
        }
    }

    private static MemoryStream RandomAsset(int length)
    {
        var stream = new MemoryStream(length);

        var random = new Random();

        for (var i = 0; i < length; i++)
        {
            stream.WriteByte((byte)random.Next());
        }

        stream.Position = 0;

        return stream;
    }

    private static string RandomName(string extension)
    {
        var fileName = $"{Guid.NewGuid()}{extension}";

        return fileName;
    }
}
