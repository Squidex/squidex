// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Ast;
using Parlot.Fluent;

namespace Squidex.Domain.Apps.Core.Templates;

public sealed class CustomFluidParser : FluidParser
{
    public Deferred<Expression> PrimaryParser => Primary;

    public Parser<char> CommaParser => Comma;
}
