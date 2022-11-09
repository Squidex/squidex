// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Core.HandleRules;

public static class Constants
{
    public static readonly Duration ExpirationTime = Duration.FromDays(30);

    public static readonly Duration StaleTime = Duration.FromDays(2);
}
