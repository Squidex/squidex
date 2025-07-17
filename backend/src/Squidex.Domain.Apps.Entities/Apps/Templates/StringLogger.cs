// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json;
using Squidex.CLI.Commands.Implementation;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

// Use newtonsoft JSON because the CLI still deals with this library and uses JTokens.
public sealed class StringLogger : ILogger, ILogLine
{
    private const int MaxActionLength = 40;
    private readonly List<string> lines = [];
    private readonly List<string> errors = [];
    private string startedLine = string.Empty;

    public bool CanWriteToSameLine => false;

    public List<string> Lines => lines;

    public void Flush(ISemanticLog log, string template)
    {
        var mesage = string.Join('\n', Lines);

        log.LogInformation(w => w
            .WriteProperty("message", $"CLI executed or template {template}.")
            .WriteProperty("template", template)
            .WriteArray("steps", a =>
            {
                foreach (var line in Lines)
                {
                    a.WriteValue(line);
                }
            }));

        if (errors.Count > 0)
        {
            throw new DomainException($"Template failed with {errors[0]}");
        }
    }

    public void Dispose()
    {
    }

    public void StepStart(string message)
    {
        if (message.Length > MaxActionLength - 3)
        {
            var length = MaxActionLength - 3;

            message = message[..length];
        }

        startedLine = $"{message.PadRight(MaxActionLength, '.')}...";
    }

    public void StepSuccess(string? details = null)
    {
        if (!string.IsNullOrWhiteSpace(details))
        {
            AddToLine($"succeeded ({details}).");
        }
        else
        {
            AddToLine("succeeded");
        }
    }

    public void StepFailed(string reason)
    {
        AddToErrors(reason);
        AddToLine($"failed: {reason.TrimEnd('.')}.");
    }

    public void StepSkipped(string reason)
    {
        AddToLine($"skipped: {reason.TrimEnd('.')}.");
    }

    public void WriteLine()
    {
        Lines.Add(string.Empty);
    }

    public void WriteLine(string message)
    {
        Lines.Add(message);
    }

    public void WriteLine(string message, params object?[] args)
    {
        Lines.Add(string.Format(CultureInfo.InvariantCulture, message, args));
    }

    public void WriteJson(object message)
    {
        Lines.Add(JsonConvert.SerializeObject(message, Formatting.Indented));
    }

    private void AddToErrors(string reason)
    {
        errors.Add(reason);
    }

    private void AddToLine(string message)
    {
        startedLine += message;
        Lines.Add(startedLine);
        startedLine = string.Empty;
    }

    public ILogLine WriteSameLine()
    {
        return this;
    }
}
