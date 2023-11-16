// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Templates;

[Serializable]
public class TemplateParseException : Exception
{
    public string Error { get; set; }

    public TemplateParseException(string template, string error, Exception? inner = null)
        : base(BuildErrorMessage(error, template), inner)
    {
        Error = error;
    }

    private static string BuildErrorMessage(string error, string template)
    {
        return $"Failed to parse template with <{error}>, template: {template}.";
    }
}
