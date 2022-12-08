// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Users;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Profile;

public class ChangeProfileModel
{
    [LocalizedRequired]
    public string Email { get; set; }

    [LocalizedRequired]
    public string DisplayName { get; set; }

    public bool IsHidden { get; set; }

    public UserValues ToValues()
    {
        return new UserValues { Email = Email, DisplayName = DisplayName, Hidden = IsHidden };
    }
}
