// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using System.Text;

namespace Squidex.Domain.Apps.Core.Templates;

[Serializable]
public class TemplateParseException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public TemplateParseException(string template, IEnumerable<string> errors, Exception? inner = null)
        : base(BuildErrorMessage(errors, template), inner)
    {
        Errors = errors.ToList();
    }

    protected TemplateParseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Errors = (info.GetValue(nameof(Errors), typeof(List<string>)) as List<string>) ?? new List<string>();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Errors), Errors.ToList());
    }

    private static string BuildErrorMessage(IEnumerable<string> errors, string template)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Failed to parse template");

        foreach (var error in errors)
        {
            sb.Append(" * ");
            sb.AppendLine(error);
        }

        sb.AppendLine();
        sb.AppendLine("Template:");
        sb.AppendLine(template);

        return sb.ToString();
    }
}
