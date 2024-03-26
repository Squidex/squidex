// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.Extensions.Options;
using Squidex.AI;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Hosting;
using Squidex.Text.Translations;
using Squidex.Web;

namespace Squidex.Config.Domain;

public static class FontendServices
{
    public static void AddSquidexFrontend(this IServiceCollection services)
    {
        services.Configure<MyUIOptions>((services, options) =>
        {
            var jsonOptions = services.GetRequiredService<JsonSerializerOptions>();

            using var jsonDocument = JsonSerializer.SerializeToDocument(options, jsonOptions);

            if (services.GetService<ExposedValues>() is ExposedValues values)
            {
                options.More["info"] = values.ToString();
            }
        });

        services.Configure<MyUIOptions>((services, options) =>
        {
            var notifo = services.GetRequiredService<IOptions<NotifoOptions>>().Value;

            if (notifo.IsConfigured())
            {
                options.More["notifoApi"] = notifo.ApiUrl;
            }
        });

        services.Configure<MyUIOptions>((services, options) =>
        {
            var translator = services.GetRequiredService<ITranslator>();

            options.More["canUseTranslator"] = translator.IsConfigured;
        });

        services.Configure<MyUIOptions>((services, options) =>
        {
            var chatAgent = services.GetRequiredService<IChatAgent>();

            options.More["canUseChatBot"] = chatAgent.IsConfigured;
        });

        services.Configure<MyUIOptions>((services, options) =>
        {
            if (string.IsNullOrWhiteSpace(options.CollaborationService))
            {
                var urlGenerator = services.GetRequiredService<IUrlGenerator>();

                var collaborationUrl =
                    urlGenerator.BuildUrl()
                        .Replace("https://", "wss://", StringComparison.OrdinalIgnoreCase)
                        .Replace("http://", "ws://", StringComparison.OrdinalIgnoreCase);

                options.CollaborationService = collaborationUrl;
            }
        });
    }
}
