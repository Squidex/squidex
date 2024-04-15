// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.States;

public interface IShardingStrategy
{
    string GetShardKey<T>(T key) where T : notnull, IDeterministicHashCode;

    IEnumerable<string> GetShardKeys();
}

public sealed class SingleSharding : IShardingStrategy
{
    public static readonly IShardingStrategy Instance = new SingleSharding();

    private SingleSharding()
    {
    }

    public string GetShardKey<T>(T key) where T : notnull, IDeterministicHashCode
    {
        return string.Empty;
    }

    public IEnumerable<string> GetShardKeys()
    {
        yield return string.Empty;
    }
}

public sealed class PartitionedSharding : IShardingStrategy
{
    private readonly int numPartitions;

    public PartitionedSharding(int numPartitions)
    {
        this.numPartitions = numPartitions;
    }

    public string GetShardKey<T>(T key) where T : notnull, IDeterministicHashCode
    {
        var partition = Math.Abs(key.GetDeterministicHashCode()) % numPartitions;

        return GetShardKey(partition);
    }

    public IEnumerable<string> GetShardKeys()
    {
        for (var i = 0; i < numPartitions; i++)
        {
            yield return GetShardKey(i);
        }
    }

    private static string GetShardKey(int partition)
    {
        return $"_{partition}";
    }
}
