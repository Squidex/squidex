// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure
{
    public sealed class LanguagesInitializer : IInitializable
    {
        private readonly LanguagesOptions options;

        public LanguagesInitializer(IOptions<LanguagesOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options.Value;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            foreach (var kvp in options)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                {
                    Language.AddLanguage(kvp.Key, kvp.Value);
                }
            }

            return TaskHelper.Done;
        }
    }
}
