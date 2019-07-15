// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ClientsDto : Resource
    {
        /// <summary>
        /// The clients.
        /// </summary>
        [Required]
        public ClientDto[] Items { get; set; }

        public static ClientsDto FromApp(IAppEntity app, ApiController controller)
        {
            var result = new ClientsDto
            {
                Items = app.Clients.Select(x => ClientDto.FromClient(x.Key, x.Value, controller, app.Name)).ToArray()
            };

            return result.CreateLinks(controller, app.Name);
        }

        private ClientsDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppClientsController>(x => nameof(x.GetClients), values));

            if (controller.HasPermission(Permissions.AppClientsCreate, app))
            {
                AddPostLink("create", controller.Url<AppClientsController>(x => nameof(x.PostClient), values));
            }

            return this;
        }
    }
}
