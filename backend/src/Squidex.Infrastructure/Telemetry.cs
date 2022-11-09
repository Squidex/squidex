// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Squidex.Infrastructure;

public static class Telemetry
{
    public static readonly ActivitySource Activities = new ActivitySource("Squidex");

    public static Activity? StartSubActivity(this ActivitySource activity, string name)
    {
        if (Activity.Current == null)
        {
            return null;
        }

        return activity.StartActivity(name);
    }
}
