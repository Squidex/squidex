// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Email;

public sealed record EmailAction : RuleAction<EmailStep>
{
    public string ServerHost { get; set; }

    public int ServerPort { get; set; }

    public string ServerUsername { get; set; }

    public string ServerPassword { get; set; }

    public string MessageFrom { get; set; }

    public string MessageTo { get; set; }

    public string MessageSubject { get; set; }

    public string MessageBody { get; set; }
}
