﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class FileTypeTagGeneratorTests
    {
        private readonly HashSet<string> tags = new HashSet<string>();
        private readonly FileTypeTagGenerator sut = new FileTypeTagGenerator();

        [Fact]
        public async Task Should_not_add_tag_if_no_file_info()
        {
            var command = new CreateAsset();

            await sut.EnhanceAsync(command, tags);

            Assert.Empty(tags);
        }

        [Fact]
        public async Task Should_add_file_type()
        {
            var command = new CreateAsset
            {
                File = new AssetFile("File.DOCX", "Mime", 100, () => new MemoryStream())
            };

            await sut.EnhanceAsync(command, tags);

            Assert.Contains("type/docx", tags);
        }

        [Fact]
        public async Task Should_add_blob_if_without_extension()
        {
            var command = new CreateAsset
            {
                File = new AssetFile("File", "Mime", 100, () => new MemoryStream())
            };

            await sut.EnhanceAsync(command, tags);

            Assert.Contains("type/blob", tags);
        }

        [Fact]
        public void Should_always_format_to_null()
        {
            var source = new AssetEntity();

            var formatted = sut.Format(source);

            Assert.Null(formatted);
        }
    }
}
