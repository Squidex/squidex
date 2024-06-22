// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.AI;

namespace Squidex.Domain.Apps.Entities;

public sealed class AppChatContext : ChatContext
{
    required public Context BaseContext { get; init; }
}
