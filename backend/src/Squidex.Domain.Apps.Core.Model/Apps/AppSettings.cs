// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record AppSettings
    {
        public static readonly AppSettings Empty = new AppSettings();

        public ImmutableList<Pattern> Patterns { get; init; } = ImmutableList.Empty<Pattern>();

        public ImmutableList<Editor> Editors { get; init; } = ImmutableList.Empty<Editor>();

        public bool HideScheduler { get; init; }

        public bool HideDateTimeModeButton { get; init; }
    }
}
