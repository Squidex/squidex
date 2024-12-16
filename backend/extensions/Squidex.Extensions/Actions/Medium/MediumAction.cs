// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Medium;

public sealed record MediumAction : RuleAction<MediumStep>
{
    public string AccessToken { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? Tags { get; set; }

    public string? PublicationId { get; set; }

    public bool IsHtml { get; set; }
}
