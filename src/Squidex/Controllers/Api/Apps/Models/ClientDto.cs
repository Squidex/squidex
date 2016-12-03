// ==========================================================================
//  ClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
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
        /// The date and time when the client key expires.
        /// </summary>
        [Required]
        public DateTime ExpiresUtc { get; set; }
    }
}
