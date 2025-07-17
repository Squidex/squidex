// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.CLI.Configuration;
using Squidex.ClientLibrary;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class SessionFactory(IOptions<TemplatesOptions> templateOptions, IUrlGenerator urlGenerator)
{
    private readonly TemplatesOptions options = templateOptions.Value;

    public Session CreateSession(App app)
    {
        var client = app.Clients.First();

        var url = options.LocalUrl;
        if (string.IsNullOrEmpty(url))
        {
            url = urlGenerator.Root();
        }

        return new Session(
            new DirectoryInfo(Path.GetTempPath()),
            new SquidexClient(new SquidexOptions
            {
                IgnoreSelfSignedCertificates = true,
                AppName = app.Name,
                ClientId = $"{app.Name}:{client.Key}",
                ClientSecret = client.Value.Secret,
                Url = url,
            }));
    }
}
