// ==========================================================================
//  ClientKeyDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class ClientKeyDto
    {
        /// <summary>
        /// The client key. 
        /// </summary>
        [Required]
        public string ClientKey { get; set; }

        /// <summary>
        /// The date and time when the client key expires.
        /// </summary>
        [Required]
        public DateTime ExpiresUtc { get; set; }
    }
}
