// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Squidex.Infrastructure
{
    public static class Telemetry
    {
        public static readonly ActivitySource Activities = new ActivitySource("Squidex");

        public static Activity? StartMethod(this ActivitySource activity, Type type, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{type.Name}/{memberName}");
        }

        public static Activity? StartMethod<T>(this ActivitySource activity, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{typeof(T).Name}/{memberName}");
        }

        public static Activity? StartMethod(this ActivitySource activity, string objectName, [CallerMemberName] string? memberName = null)
        {
            return activity.StartActivity($"{objectName}/{memberName}");
        }
    }
}
