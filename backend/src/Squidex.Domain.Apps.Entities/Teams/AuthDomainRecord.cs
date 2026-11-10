// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams;

public static class AuthDomainRecord
{
    public static string GetName(string domain)
    {
        return $"_squidex.{domain}";
    }

    public static string GetValue(DomainId teamId)
    {
        return $"squidex-team={teamId}";
    }
}
