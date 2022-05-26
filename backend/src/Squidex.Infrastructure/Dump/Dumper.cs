﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Hosting;

namespace Squidex.Infrastructure.Dump
{
    public sealed class Dumper : IInitializable
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(30);
        private readonly DumperOptions options;
        private readonly IAssetStore assetStore;
        private Task? scheduledGcDumpTask;
        private Task? scheduledDumpTask;
        private Timer? timer;

        public int Order => int.MaxValue;

        public Dumper(IOptions<DumperOptions> options, IAssetStore assetStore)
        {
            this.options = options.Value;
            this.assetStore = assetStore;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            if (options.DumpLimit > 0 || options.GCDumpLimit > 0)
            {
                timer = new Timer(CollectDump);
                timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }

            return Task.CompletedTask;
        }

        public Task ReleaseAsync(
            CancellationToken ct)
        {
            var tasks = new List<Task>();

            if (timer != null)
            {
                tasks.Add(timer.DisposeAsync().AsTask());
            }

            if (scheduledDumpTask != null)
            {
                tasks.Add(scheduledDumpTask);
            }

            if (scheduledGcDumpTask != null)
            {
                tasks.Add(scheduledGcDumpTask);
            }

            return Task.WhenAll(tasks);
        }

        private void CollectDump(object? state)
        {
            try
            {
                var usage = GC.GetTotalMemory(false) / (1024 * 1024);

                if (options.DumpLimit > 0 && usage > options.DumpLimit && scheduledDumpTask == null)
                {
                    scheduledDumpTask = CreateDumpAsync();
                }

                if (options.GCDumpLimit > 0 && usage > options.GCDumpLimit && scheduledGcDumpTask == null)
                {
                    scheduledGcDumpTask = CreateGCDumpAsync();
                }
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            {

            }
        }

        public Task<bool> CreateDumpAsync(
            CancellationToken ct = default)
        {
            return CreateDumpAsync(options.DumpTool, "dump", ct);
        }

        public Task<bool> CreateGCDumpAsync(
            CancellationToken ct = default)
        {
            return CreateDumpAsync(options.GcDumpTool, "gcdump", ct);
        }

        private async Task<bool> CreateDumpAsync(string? tool, string extension,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tool))
            {
                return false;
            }

            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var ctl = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            var writtenFile = $"{tempPath}.{extension}";
            try
            {
                using var process = new Process();
                process.StartInfo.Arguments = $"collect -p {Environment.ProcessId} -o {tempPath}";
                process.StartInfo.FileName = tool;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                await process.WaitForExitAsync(ctl.Token);

                var isSucceess = process.ExitCode == 0;

                if (!isSucceess)
                {
                    return false;
                }

                await using (var fs = new FileStream(writtenFile, FileMode.Open))
                {
                    var name = $"diagnostics/{extension}/{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}.{extension}";

                    await assetStore.UploadAsync(name, fs, true, ctl.Token);
                }
            }
            finally
            {
                try
                {
                    File.Delete(tempPath);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                }
            }

            return true;
        }
    }
}
