// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Roles = Squidex.Domain.Apps.Core.Apps.Role;

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
        /// The role of the client.
        /// </summary>
        public string Role { get; set; }

        public static ClientDto FromKvp(KeyValuePair<string, AppClient> kvp)
        {
            return SimpleMapper.Map(kvp.Value, new ClientDto { Id = kvp.Key });
        }

        public static ClientDto FromCommand(AttachClient command)
        {
            return SimpleMapper.Map(command, new ClientDto { Name = command.Id, Role = Roles.Editor });
        }
    }
}
