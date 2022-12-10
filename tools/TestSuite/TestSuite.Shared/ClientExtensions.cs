// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

namespace TestSuite;

public static class ClientExtensions
{
    public static async Task<bool> WaitForDeletionAsync(this IAssetsClient assetsClient, string app, string id, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await assetsClient.GetAssetAsync(app, id, cts.Token);
                }
                catch (SquidexManagementException ex) when (ex.StatusCode == 404)
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

    public static async Task<ContentsResult<TEntity, TData>> WaitForContentAsync<TEntity, TData>(this IContentsClient<TEntity, TData> contentsClient, ContentQuery q, Func<TEntity, bool> predicate, TimeSpan timeout) where TEntity : Content<TData> where TData : class, new()
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await contentsClient.GetAsync(q, null, cts.Token);

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

    public static async Task<IList<SearchResultDto>> WaitForSearchAsync(this ISearchClient searchClient, string app, string query, Func<SearchResultDto, bool> predicate, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await searchClient.GetSearchResultsAsync(app, query, cts.Token);

                if (result.Any(predicate))
                {
                    return result.ToList();
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return new List<SearchResultDto>();
    }

    public static async Task<IList<HistoryEventDto>> WaitForHistoryAsync(this IHistoryClient historyClient, string app, string channel, Func<HistoryEventDto, bool> predicate, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await historyClient.GetAppHistoryAsync(app, channel, cts.Token);

                if (result.Any(predicate))
                {
                    return result.ToList();
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return new List<HistoryEventDto>();
    }

    public static async Task<IDictionary<string, int>> WaitForTagsAsync(this IAssetsClient assetsClient, string app, string id, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await assetsClient.GetTagsAsync(app, cts.Token);

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

        return await assetsClient.GetTagsAsync(app);
    }

    public static async Task<IList<BackupJobDto>> WaitForBackupsAsync(this IBackupsClient backupsClient, string app, Func<BackupJobDto, bool> predicate, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await backupsClient.GetBackupsAsync(app, cts.Token);

                if (result.Items.Any(predicate))
                {
                    return result.Items;
                }

                await Task.Delay(200, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return null;
    }

    public static async Task<RestoreJobDto> WaitForRestoreAsync(this IBackupsClient backupsClient, Func<RestoreJobDto, bool> predicate, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            while (!cts.IsCancellationRequested)
            {
                var result = await backupsClient.GetRestoreJobAsync(cts.Token);

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

    public static async Task<MemoryStream> DownloadAsync(this ClientFixture fixture, AssetDto asset, int? version = null)
    {
        var temp = new MemoryStream();

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(fixture.Url);

            var url = asset._links["content"].Href[1..];

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

    public static async Task<AssetDto> UploadFileAsync(this IAssetsClient assetsClients, string app, string path, AssetDto asset, string fileName = null)
    {
        var fileInfo = new FileInfo(path);

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileName ?? fileInfo.Name, asset.MimeType);

            return await assetsClients.PutAssetContentAsync(app, asset.Id, upload);
        }
    }

    public static async Task<AssetDto> UploadFileAsync(this IAssetsClient assetsClients, string app, string path, string fileType, string fileName = null, string parentId = null, string id = null)
    {
        var fileInfo = new FileInfo(path);

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileName ?? fileInfo.Name, fileType);

            return await assetsClients.PostAssetAsync(app, parentId, id, true, upload);
        }
    }

    public static async Task<AssetDto> UploadRandomFileAsync(this IAssetsClient assetsClients, string app, int size, string parentId = null, string id = null)
    {
        using (var stream = RandomAsset(size))
        {
            var upload = new FileParameter(stream, RandomName(".txt"), "text/csv");

            return await assetsClients.PostAssetAsync(app, parentId, id, true, upload);
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
