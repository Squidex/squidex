// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class ScriptLog : IDisposable
{
    public const int MaxEntries = 100;
    public const int MaxLength = 1000;
    private static readonly AsyncLocal<ScriptLog?> CurrentLog = new AsyncLocal<ScriptLog?>();
    private readonly List<ScriptLogEntry> entries = [];
    private readonly List<Action<ScriptLog>> completedCallbacks = [];
    private readonly ScriptLog? previous;
    private int totalEntries;

    public string Name { get; }

    public bool Persist { get; }

    public static ScriptLog? Current
    {
        get => CurrentLog.Value;
    }

    public bool HasEntries
    {
        get
        {
            lock (entries)
            {
                return entries.Count > 0;
            }
        }
    }

    private ScriptLog(string name, bool persist, ScriptLog? previous)
    {
        Name = name;
        Persist = persist;

        this.previous = previous;
    }

    public static ScriptLog Begin(string name, bool persist = false)
    {
        Guard.NotNullOrEmpty(name);

        var log = new ScriptLog(name, persist, CurrentLog.Value);

        CurrentLog.Value = log;
        return log;
    }

    public void Dispose()
    {
        if (CurrentLog.Value == this)
        {
            CurrentLog.Value = previous;
        }

        Action<ScriptLog>[] callbacks;

        lock (entries)
        {
            callbacks = completedCallbacks.ToArray();
            completedCallbacks.Clear();
        }

        foreach (var callback in callbacks)
        {
            callback(this);
        }
    }

    public void OnCompleted(Action<ScriptLog> callback)
    {
        lock (entries)
        {
            completedCallbacks.Add(callback);
        }
    }

    public void Add(string level, string message)
    {
        // Scripts can log in loops, therefore the log is limited to protect the memory.
        if (message.Length > MaxLength)
        {
            message = $"{message[..MaxLength]}...";
        }

        lock (entries)
        {
            totalEntries++;

            if (entries.Count < MaxEntries)
            {
                entries.Add(new ScriptLogEntry(SystemClock.Instance.GetCurrentInstant(), level, message));
            }
        }
    }

    public IReadOnlyList<string> ToLines()
    {
        lock (entries)
        {
            var lines = entries.Select(x => x.ToString()).ToList();

            if (totalEntries > entries.Count)
            {
                lines.Add($"... {totalEntries - entries.Count} more entries omitted.");
            }

            return lines;
        }
    }

    public (IReadOnlyList<ScriptLogEntry> Entries, int TotalEntries) Snapshot()
    {
        lock (entries)
        {
            return (entries.ToList(), totalEntries);
        }
    }

    public static async Task<T> CollectAsync<T>(string name, Func<Task<T>> action)
    {
        using var log = Begin(name, true);

        return await action();
    }

    public static Task CollectAsync(string name, Func<Task> action)
    {
        return CollectAsync(name, async () =>
        {
            await action();
            return true;
        });
    }
}
