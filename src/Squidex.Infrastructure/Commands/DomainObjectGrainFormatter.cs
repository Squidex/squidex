// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;

namespace Squidex.Infrastructure.Commands
{
    public static class DomainObjectGrainFormatter
    {
        public static string Format(IGrainCallContext context)
        {
            if (context.Method == null)
            {
                return "Unknown";
            }

            if (string.Equals(context.Method.Name, nameof(IDomainObjectGrain.ExecuteAsync), StringComparison.CurrentCultureIgnoreCase) &&
                context.Arguments?.Length == 1 &&
                context.Arguments[0] != null)
            {
                var argumentFullName = context.Arguments[0].ToString();
                var argumentParts = argumentFullName.Split('.');
                var argumentName = argumentParts[argumentParts.Length - 1];

                return $"{nameof(IDomainObjectGrain.ExecuteAsync)}({argumentName})";
            }

            return context.Method.Name;
        }
    }
}
