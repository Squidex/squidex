// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public enum TextState
{
    Stage0_Draft__Stage1_None,
    Stage0_Published__Stage1_None,
    Stage0_Published__Stage1_Draft,
    Stage1_Draft__Stage0_None,
    Stage1_Published__Stage0_None,
    Stage1_Published__Stage0_Draft,
    Deleted,
}
