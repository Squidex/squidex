// ==========================================================================
//  ConfigureScriptsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Schemas.Models
{
    public sealed class ConfigureScriptsDto
    {
        /// <summary>
        /// The script that is executed for each query when querying contents.
        /// </summary>
        public string ScriptQuery { get; set; }

        /// <summary>
        /// The script that is executed when creating a content.
        /// </summary>
        public string ScriptCreate { get; set; }

        /// <summary>
        /// The script that is executed when updating a content.
        /// </summary>
        public string ScriptUpdate { get; set; }

        /// <summary>
        /// The script that is executed when deleting a content.
        /// </summary>
        public string ScriptDelete { get; set; }

        /// <summary>
        /// The script that is executed when publishing a content.
        /// </summary>
        public string ScriptPublish { get; set; }

        /// <summary>
        /// The script that is executed when unpublishing a content.
        /// </summary>
        public string ScriptUnpublish { get; set; }
    }
}
