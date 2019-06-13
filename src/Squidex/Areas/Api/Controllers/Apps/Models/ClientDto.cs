// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ClientDto : Resource
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

        public static ClientDto FromClient(string id, AppClient client, ApiController controller, string app)
        {
            var result = SimpleMapper.Map(client, new ClientDto { Id = id });

            return CreateLinks(result, controller, app);
        }

        private static ClientDto CreateLinks(ClientDto result, ApiController controller, string app)
        {
            return result;
        }
    }
}
