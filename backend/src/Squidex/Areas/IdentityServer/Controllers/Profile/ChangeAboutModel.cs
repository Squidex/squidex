// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile;

public class ChangeAboutModel
{
    public string? CompanyRole { get; set; }

    public string? CompanySize { get; set; }

    public string? Project { get; set; }

    public UserValues ToValues()
    {
        return new UserValues
        {
            Answers = new Dictionary<string, string?>
            {
                ["companyRole"] = CompanyRole,
                ["companySize"] = CompanySize,
                ["project"] = Project,
            }
        };
    }
}
