// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class ClientDto : Resource
{
    /// <summary>
    /// The client id.
    /// </summary>
    [LocalizedRequired]
    public string Id { get; set; }

    /// <summary>
    /// The client secret.
    /// </summary>
    [LocalizedRequired]
    public string Secret { get; set; }

    /// <summary>
    /// The client name.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The role of the client.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// The number of allowed api calls per month for this client.
    /// </summary>
    public long ApiCallsLimit { get; set; }

    /// <summary>
    /// The number of allowed api traffic bytes per month for this client.
    /// </summary>
    public long ApiTrafficLimit { get; set; }

    /// <summary>
    /// True to allow anonymous access without an access token for this client.
    /// </summary>
    public bool AllowAnonymous { get; set; }

    public static ClientDto FromClient(string id, AppClient client)
    {
        var result = SimpleMapper.Map(client, new ClientDto { Id = id });

        return result;
    }

    public ClientDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, id = Id };

        if (resources.CanUpdateClient)
        {
            AddPutLink("update",
                resources.Url<AppClientsController>(x => nameof(x.PutClient), values));
        }

        if (resources.CanDeleteClient)
        {
            AddDeleteLink("delete",
                resources.Url<AppClientsController>(x => nameof(x.DeleteClient), values));
        }

        return this;
    }
}
