// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AppSettings
    {
        public static readonly AppSettings Empty = new AppSettings();

        public ReadOnlyCollection<Pattern> Patterns { get; init; } = ReadOnlyCollection.Empty<Pattern>();

        public ReadOnlyCollection<Editor> Editors { get; init; } = ReadOnlyCollection.Empty<Editor>();

        public bool HideScheduler { get; init; }

        public bool HideDateTimeModeButton { get; init; }

        public bool HideDateTimeQuickButtons { get; init; }
    }
}
