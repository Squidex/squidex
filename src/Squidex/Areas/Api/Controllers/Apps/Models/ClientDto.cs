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
    public sealed class ClientDto
    {
        /// <summary>
        /// The client id.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The client secret.
        /// </summary>
        [Required]
        public string Secret { get; set; }

        /// <summary>
        /// The client name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The permissions of the client.
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public AppClientPermission Permission { get; set; }
    }
}
