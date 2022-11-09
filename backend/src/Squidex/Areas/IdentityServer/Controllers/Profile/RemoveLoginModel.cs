// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Profile;

public class RemoveLoginModel
{
    [LocalizedRequired]
    public string LoginProvider { get; set; }

    [LocalizedRequired]
    public string ProviderKey { get; set; }
}
