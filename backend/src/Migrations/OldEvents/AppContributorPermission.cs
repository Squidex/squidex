// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Migrations.OldEvents
{
    [Obsolete("New Event introduced")]
    public enum AppContributorPermission
    {
        Owner,
        Developer,
        Editor
    }
}
