// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Templates;

[Serializable]
public class TemplateParseException(string template, string error, Exception? inner = null) : Exception(BuildErrorMessage(error, template), inner)
{
    public string Error { get; set; } = error;

    private static string BuildErrorMessage(string error, string template)
    {
        return $"Failed to parse template with <{error}>, template: {template}.";
    }
}
