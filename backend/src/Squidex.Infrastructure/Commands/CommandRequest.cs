// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Commands;

public sealed class CommandRequest(IAggregateCommand command, string culture, string cultureUI)
{
    public IAggregateCommand Command { get; } = command;

    public string Culture { get; } = culture;

    public string CultureUI { get; } = cultureUI;

    public static CommandRequest Create(IAggregateCommand command)
    {
        return new CommandRequest(command,
            CultureInfo.CurrentCulture.Name,
            CultureInfo.CurrentUICulture.Name);
    }

    public void ApplyContext()
    {
        var currentCulture = GetCulture(Culture);

        if (currentCulture != null)
        {
            CultureInfo.CurrentCulture = currentCulture;
        }

        var currentUICulture = GetCulture(CultureUI);

        if (currentUICulture != null)
        {
            CultureInfo.CurrentUICulture = currentUICulture;
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
