// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents;

[TypeName(nameof(NoopEvent))]
public sealed class NoopEvent : SquidexEvent
{
}
