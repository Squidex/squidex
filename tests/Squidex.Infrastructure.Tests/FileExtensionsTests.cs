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

        [Theory]
        [InlineData(-1, "")]
        [InlineData(-2, "")]
        [InlineData(0, "0 bytes")]
        [InlineData(50, "50 bytes")]
        [InlineData(1024, "1 kB")]
        [InlineData(870400, "850 kB")]
        [InlineData(1572864, "1.5 MB")]
        [InlineData(4294967296, "4 GB")]
        [InlineData(3408486046105, "3.1 TB")]
        [InlineData(3490289711212134, "3174.4 TB")]
        public void Should_calculate_file_size(long bytes, string expected)
        {
            var actual = bytes.ToReadableSize();

            Assert.Equal(expected, actual);
        }
    }
}
