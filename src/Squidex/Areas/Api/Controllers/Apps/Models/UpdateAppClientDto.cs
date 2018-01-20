// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateAppClientDto
    {
        /// <summary>
        /// The new display name of the client.
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// The permissions of the client.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AppClientPermission? Permission { get; set; }
    }
}
