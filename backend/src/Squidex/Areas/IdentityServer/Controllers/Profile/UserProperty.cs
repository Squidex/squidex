// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public sealed class UserProperty
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Value is required.")]
        public string Value { get; set; }

        public (string Name, string Value) ToTuple()
        {
            return (Name, Value);
        }

        public static UserProperty FromTuple((string Name, string Value) value)
        {
            return new UserProperty { Name = value.Name, Value = value.Value };
        }
    }
}
