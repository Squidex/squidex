// ==========================================================================
//  ListAppDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Core.Apps;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class AppDto
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the app.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The date and time when the app has been created.
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// The date and time when the app has been modified last.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// The permission level of the user.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PermissionLevel Permission { get; set; }
    }
}
