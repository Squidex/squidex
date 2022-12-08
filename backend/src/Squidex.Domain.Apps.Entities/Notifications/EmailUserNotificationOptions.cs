// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Notifications;

public sealed class EmailUserNotificationOptions
{
    public string UsageSubject { get; set; }

    public string UsageBody { get; set; }

    public string NewUserSubject { get; set; }

    public string NewUserBody { get; set; }

    public string ExistingUserSubject { get; set; }

    public string ExistingUserBody { get; set; }

    public string NewTeamUserSubject { get; set; }

    public string NewTeamUserBody { get; set; }

    public string ExistingTeamUserSubject { get; set; }

    public string ExistingTeamUserBody { get; set; }
}
