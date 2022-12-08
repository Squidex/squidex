// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;

namespace Squidex.Infrastructure.EventSourcing;

[Serializable]
public class WrongEventVersionException : Exception
{
    public long CurrentVersion { get; }

    public long ExpectedVersion { get; }

    public WrongEventVersionException(long currentVersion, long expectedVersion, Exception? inner = null)
        : base(FormatMessage(currentVersion, expectedVersion), inner)
    {
        CurrentVersion = currentVersion;

        ExpectedVersion = expectedVersion;
    }

    protected WrongEventVersionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        CurrentVersion = info.GetInt64(nameof(CurrentVersion));

        ExpectedVersion = info.GetInt64(nameof(ExpectedVersion));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(CurrentVersion), CurrentVersion);
        info.AddValue(nameof(ExpectedVersion), ExpectedVersion);

        base.GetObjectData(info, context);
    }

    private static string FormatMessage(long currentVersion, long expectedVersion)
    {
        return $"Requested version {expectedVersion}, but found {currentVersion}.";
    }
}
