// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangePropertiesModel
    {
        public List<UserProperty> Properties { get; set; }

        public UserValues ToValues()
        {
            var properties = Properties?.Select(x => x.ToTuple()).ToList() ?? new List<(string Name, string Value)>();

            return new UserValues { Properties = properties };
        }
    }
}
