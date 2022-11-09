// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Commands;

public sealed class CommandRequest
{
    public IAggregateCommand Command { get; }

    public string Culture { get; }

    public string CultureUI { get; }

    public CommandRequest(IAggregateCommand command, string culture, string cultureUI)
    {
        Command = command;

        Culture = culture;
        CultureUI = cultureUI;
    }

    public static CommandRequest Create(IAggregateCommand command)
    {
        return new CommandRequest(command,
            CultureInfo.CurrentCulture.Name,
            CultureInfo.CurrentUICulture.Name);
    }

    public void ApplyContext()
    {
        var culture = GetCulture(Culture);

        if (culture != null)
        {
            CultureInfo.CurrentCulture = culture;
        }

        var uiCulture = GetCulture(CultureUI);

        if (uiCulture != null)
        {
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }

    private static CultureInfo? GetCulture(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
