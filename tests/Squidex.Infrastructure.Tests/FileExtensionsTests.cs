// ==========================================================================
//  FileExtensionsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class FileExtensionsTests
    {
        [Theory]
        [InlineData("test.mp4", "mp4")]
        [InlineData("test.MP4", "mp4")]
        [InlineData("test.txt", "txt")]
        [InlineData("test.TXT", "txt")]
        public void Should_calculate_file_type(string fileName, string expected)
        {
            var actual = fileName.FileType();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Should_blob_for_invalid_file_types(string fileName)
        {
            var actual = fileName.FileType();

            Assert.Equal("blob", actual);
        }
    }
}
