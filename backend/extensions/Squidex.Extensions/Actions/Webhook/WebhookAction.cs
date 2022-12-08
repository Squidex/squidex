// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Webhook;

[RuleAction(
    Title = "Webhook",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M5.95 27.125h-.262C1.75 26.425 0 23.187 0 20.3c0-2.713 1.575-5.688 5.075-6.563V9.712c0-.525.35-.875.875-.875s.875.35.875.875v4.725c0 .438-.35.787-.7.875-2.975.438-4.375 2.8-4.375 4.988s1.313 4.55 4.2 5.075h.175a.907.907 0 0 1 .7 1.05c-.088.438-.438.7-.875.7zM21.175 27.387c-2.8 0-5.775-1.662-6.65-5.075H9.712c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h5.512c.438 0 .787.35.875.7.438 2.975 2.8 4.288 4.988 4.375 2.188 0 4.55-1.313 5.075-4.2v-.088a.908.908 0 0 1 1.05-.7.908.908 0 0 1 .7 1.05v.088c-.612 3.85-3.85 5.6-6.737 5.6zM21.525 18.55c-.525 0-.875-.35-.875-.875v-4.813c0-.438.35-.787.7-.875 2.975-.438 4.288-2.8 4.375-4.987 0-2.188-1.313-4.55-4.2-5.075h-.088c-.525-.175-.875-.613-.787-1.05s.525-.788 1.05-.7h.088c3.938.7 5.688 3.937 5.688 6.825 0 2.713-1.662 5.688-5.075 6.563v4.113c0 .438-.438.875-.875.875zM1.137 6.737H.962c-.438-.087-.788-.525-.7-.963v-.087c.7-3.938 3.85-5.688 6.737-5.688h.087c2.712 0 5.688 1.662 6.563 5.075h4.025c.525 0 .875.35.875.875s-.35.875-.875.875h-4.725c-.438 0-.788-.35-.875-.7-.438-2.975-2.8-4.288-4.988-4.375-2.188 0-4.55 1.313-5.075 4.2v.087c-.088.438-.438.7-.875.7z'/><path d='M7 10.588c-.875 0-1.837-.35-2.538-1.05a3.591 3.591 0 0 1 0-5.075C5.162 3.851 6.037 3.5 7 3.5s1.838.35 2.537 1.05c.7.7 1.05 1.575 1.05 2.537s-.35 1.837-1.05 2.538c-.7.612-1.575.963-2.537.963zM7 5.25c-.438 0-.875.175-1.225.525a1.795 1.795 0 0 0 2.538 2.538c.35-.35.525-.788.525-1.313s-.175-.875-.525-1.225S7.525 5.25 7 5.25zM21.088 23.887a3.65 3.65 0 0 1-2.537-1.05 3.591 3.591 0 0 1 0-5.075c.7-.7 1.575-1.05 2.537-1.05s1.838.35 2.537 1.05c.7.7 1.05 1.575 1.05 2.538s-.35 1.837-1.05 2.537c-.787.7-1.662 1.05-2.537 1.05zm0-5.337c-.525 0-.963.175-1.313.525a1.795 1.795 0 0 0 2.537 2.538c.35-.35.525-.788.525-1.313s-.175-.963-.525-1.313-.787-.438-1.225-.438zM20.387 10.588c-.875 0-1.837-.35-2.537-1.05S16.8 7.963 16.8 7.001s.35-1.837 1.05-2.538c.7-.612 1.662-.962 2.537-.962s1.838.35 2.538 1.05c1.4 1.4 1.4 3.675 0 5.075-.7.612-1.575.963-2.538.963zm0-5.338c-.525 0-.962.175-1.313.525s-.525.788-.525 1.313.175.962.525 1.313c.7.7 1.838.7 2.538 0s.7-1.838 0-2.538c-.263-.438-.7-.612-1.225-.612zM7.087 23.887c-.875 0-1.837-.35-2.538-1.05s-1.05-1.575-1.05-2.537.35-1.838 1.05-2.538c.7-.612 1.575-.962 2.538-.962s1.837.35 2.538 1.05c1.4 1.4 1.4 3.675 0 5.075-.7.612-1.575.962-2.538.962zm0-5.337c-.525 0-.962.175-1.313.525s-.525.788-.525 1.313.175.963.525 1.313a1.794 1.794 0 1 0 2.538-2.537c-.263-.438-.7-.612-1.225-.612z'/></svg>",
    IconColor = "#4bb958",
    Display = "Send webhook",
    Description = "Invoke HTTP endpoints on a target system.",
    ReadMore = "https://en.wikipedia.org/wiki/Webhook")]
public sealed record WebhookAction : RuleAction
{
    [LocalizedRequired]
    [Display(Name = "Url", Description = "The url to the webhook.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public Uri Url { get; set; }

    [LocalizedRequired]
    [Display(Name = "Method", Description = "The type of the request.")]
    public WebhookMethod Method { get; set; }

    [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string Payload { get; set; }

    [Display(Name = "Payload Type", Description = "The mime type of the payload.")]
    [Editor(RuleFieldEditor.Text)]
    public string PayloadType { get; set; }

    [Display(Name = "Headers (Optional)", Description = "The message headers in the format '[Key]=[Value]', one entry per line.")]
    [Editor(RuleFieldEditor.TextArea)]
    public string Headers { get; set; }

    [Display(Name = "Shared Secret", Description = "The shared secret that is used to calculate the payload signature.")]
    [Editor(RuleFieldEditor.Text)]
    public string SharedSecret { get; set; }
}

public enum WebhookMethod
{
    POST,
    PUT,
    GET,
    DELETE,
    PATCH
}
