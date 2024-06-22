// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.AI;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppChatTools : IChatToolProvider
{
    private readonly IJsonSerializer serializer;
    private readonly IUrlGenerator urlGenerator;

    public AppChatTools(IJsonSerializer serializer, IUrlGenerator urlGenerator)
    {
        this.serializer = serializer;
        this.urlGenerator = urlGenerator;
    }

    public async IAsyncEnumerable<IChatTool> GetToolsAsync(ChatContext chatContext,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (chatContext is not AppChatContext appContext)
        {
            yield break;
        }

        var context = appContext.BaseContext;

        await Task.Yield();

        if (context.Allows(PermissionIds.AppClientsRead))
        {
            yield return new DelegateChatTool(
                new ToolSpec("clients", "Clients", "Provides the clients for the Squidex App."),
                (_, ct) =>
                {
                    var result = new
                    {
                        Clients = context.App.Clients.Select(x =>
                            new
                            {
                                Id = x.Key,
                                ClientId = $"{context.App.Name}:{x.Key}",
                                ClientSecret = "obfuscated",
                                x.Value.Role
                            }),
                        Url = urlGenerator.ClientsUI(context.App.NamedId())
                    };

                    var json = serializer.Serialize(result, true);

                    return Task.FromResult(json);
                });
        }

        if (context.Allows(PermissionIds.AppLanguagesRead))
        {
            yield return new DelegateChatTool(
                new ToolSpec("languages", "Languages", "Provides the languages for the Squidex App."),
                (_, ct) =>
                {
                    var result = new
                    {
                        Languages = context.App.Languages.Values.Select(x =>
                            new
                            {
                                Iso2Code = x.Key,
                                IsMaster = context.App.Languages.Master.Equals(x.Key),
                                x.Value.IsOptional
                            }),
                        Url = urlGenerator.LanguagesUI(context.App.NamedId())
                    };

                    var json = serializer.Serialize(result, true);

                    return Task.FromResult(json);
                });
        }

        if (context.Allows(PermissionIds.AppRolesRead))
        {
            yield return new DelegateChatTool(
                new ToolSpec("roles", "Roles", "Provides the roles for the Squidex App."),
                (_, ct) =>
                {
                    var result = new
                    {
                        Roles = context.App.Roles.Custom.Select(x =>
                            new
                            {
                                x.Name
                            }),
                        Url = urlGenerator.RolesUI(context.App.NamedId())
                    };

                    var json = serializer.Serialize(result, true);

                    return Task.FromResult(json);
                });
        }

        if (context.Allows(PermissionIds.AppPlansRead))
        {
            yield return new DelegateChatTool(
                new ToolSpec("plan", "Plan", "Provides the plan for the Squidex App."),
                (_, ct) =>
                {
                    var result = new
                    {
                        Plan = new
                        {
                            Name = context.App.Plan?.PlanId,
                        },
                        Url = urlGenerator.PlansUI(context.App.NamedId())
                    };

                    var json = serializer.Serialize(result, true);

                    return Task.FromResult(json);
                });
        }
    }
}
