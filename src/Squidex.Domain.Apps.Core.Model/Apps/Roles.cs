// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Roles : DictionaryWrapper<string, Role>
    {
        public static readonly Roles Empty = new Roles();

        private Roles()
            : base(ImmutableDictionary<string, Role>.Empty)
        {
        }

        public Roles(ImmutableDictionary<string, Role> inner)
            : base(inner)
        {
        }

        public static Roles CreateDefaults(string app)
        {
            return new Roles(
                new Dictionary<string, Role>
                {
                    [Role.Developer] = Role.CreateDeveloper(app),
                    [Role.Editor] = Role.CreateEditor(app),
                    [Role.Owner] = Role.CreateOwner(app),
                    [Role.Reader] = Role.CreateReader(app)
                }.ToImmutableDictionary());
        }
    }
}
