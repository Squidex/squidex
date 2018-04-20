// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
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
        /// The script that is executed when change a content status.
        /// </summary>
        public string ScriptChange { get; set; }

        public ConfigureScripts ToCommand()
        {
            return SimpleMapper.Map(this, new ConfigureScripts());
        }
    }
}
