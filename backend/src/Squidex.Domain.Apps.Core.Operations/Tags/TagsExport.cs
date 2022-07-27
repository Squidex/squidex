// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Tags
{
    public class TagsExport
    {
        private Dictionary<string, Tag> tags;
        private Dictionary<string, string> alias;

        public Dictionary<string, Tag> Tags
        {
            get => tags ??= new Dictionary<string, Tag>();
            set => tags = value ?? new Dictionary<string, Tag>();
        }

        public Dictionary<string, string> Alias
        {
            get => alias ??= new Dictionary<string, string>();
            set => alias = value ?? new Dictionary<string, string>();
        }

        public TagsExport Clone()
        {
            var clonedAlias = new Dictionary<string, string>(Alias);

            var clonedTags =
                Tags.ToDictionary(
                    x => x.Key,
                    x => x.Value with { });

            return new TagsExport { Alias = clonedAlias, Tags = clonedTags };
        }
    }
}
