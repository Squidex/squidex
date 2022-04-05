// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;

namespace Squidex.Infrastructure.Commands
{
    public static class DomainObjectGrainFormatter
    {
        public static readonly Func<IIncomingGrainCallContext, string> Format = context =>
        {
            if (context.InterfaceMethod == null)
            {
                return "Unknown";
            }

            if (string.Equals(context.InterfaceMethod.Name, nameof(IDomainObjectGrain.ExecuteAsync), StringComparison.CurrentCultureIgnoreCase) &&
                context.Arguments?.Length > 0 &&
                context.Arguments[0] != null)
            {
                var argumentFullName = context.Arguments[0].ToString();

                if (argumentFullName != null)
                {
                    var argumentParts = argumentFullName.Split('.');
                    var argumentName = argumentParts[^1];

                    return $"{nameof(IDomainObjectGrain.ExecuteAsync)}({argumentName})";
                }
            }

            return context.InterfaceMethod.Name;
        };
    }
}
