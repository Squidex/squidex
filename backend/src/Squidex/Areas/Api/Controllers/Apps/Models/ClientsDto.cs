// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ClientsDto : Resource
    {
        /// <summary>
        /// The clients.
        /// </summary>
        [LocalizedRequired]
        public ClientDto[] Items { get; set; }

        public static ClientsDto FromApp(IAppEntity app, Resources resources)
        {
            var result = new ClientsDto
            {
                Items = app.Clients
                    .Select(x => ClientDto.FromClient(x.Key, x.Value))
                    .Select(x => x.CreateLinks(resources))
                    .ToArray()
            };

            return result.CreateLinks(resources);
        }

        private ClientsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppClientsController>(x => nameof(x.GetClients), values));

            if (resources.CanCreateClient)
            {
                AddPostLink("create", resources.Url<AppClientsController>(x => nameof(x.PostClient), values));
            }

            return this;
        }
    }
}
