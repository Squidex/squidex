// ==========================================================================
//  UpdateAppClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Controllers.Api.Apps.Models
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
