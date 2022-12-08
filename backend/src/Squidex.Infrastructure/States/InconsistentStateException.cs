// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;

namespace Squidex.Infrastructure.States;

[Serializable]
public class InconsistentStateException : Exception
{
    public long VersionCurrent { get; }

    public long VersionExpected { get; }

    public InconsistentStateException(long current, long expected, Exception? inner = null)
        : base(FormatMessage(current, expected), inner)
    {
        VersionCurrent = current;
        VersionExpected = expected;
    }

    protected InconsistentStateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        VersionCurrent = info.GetInt64(nameof(VersionCurrent));
        VersionExpected = info.GetInt64(nameof(VersionExpected));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(VersionCurrent), VersionCurrent);
        info.AddValue(nameof(VersionExpected), VersionExpected);

        base.GetObjectData(info, context);
    }

    private static string FormatMessage(long current, long expected)
    {
        return $"Requested version {expected}, but found {current}.";
    }
}
