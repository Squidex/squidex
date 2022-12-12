// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.FileProviders;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Web;

public class IgnoreHashFileProviderTests
{
    private readonly IFileProvider inner = A.Fake<IFileProvider>();

    [Fact]
    public void Should_get_file_from_inner()
    {
        var fileNormal = CreateFile("styles.css");

        var sut = CreateSut();

        A.CallTo(() => inner.GetFileInfo(fileNormal.Name))
            .Returns(fileNormal);

        var actual = sut.GetFileInfo(fileNormal.Name);

        Assert.Equal(fileNormal, actual);
    }

    [Theory]
    [InlineData(@"\styles.css")]
    [InlineData(@"/styles.css")]
    public void Should_get_file_from_hashed_version_if_normal_file_does_not_exist(string path)
    {
        var fileNormal = CreateFile("styles.css", exists: false);
        var fileHashed = CreateFile("styles.42efefef.css");

        var directories = new[]
        {
            (string.Empty,
                new[]
                {
                    fileHashed
                }
            )
        };

        var sut = CreateSut(directories);

        A.CallTo(() => inner.GetFileInfo(path))
            .Returns(fileNormal);

        A.CallTo(() => inner.GetFileInfo(fileHashed.Name))
            .Returns(fileHashed);

        var actual = sut.GetFileInfo(path);

        Assert.Equal(fileHashed, actual);
    }

    [Theory]
    [InlineData(@"build/styles.css")]
    [InlineData(@"build\styles.css")]
    [InlineData(@"\build\styles.css")]
    [InlineData(@"/build/styles.css")]
    public void Should_get_nested_file_from_hashed_version_if_normal_file_does_not_exist(string path)
    {
        var directory = CreateFile("build", directory: true);

        var fileNormal = CreateFile("styles.css", exists: false);
        var fileHashed = CreateFile("styles.42efefef.css");

        var directories = new[]
        {
            (string.Empty,
                new[]
                {
                    directory
                }
            ),
            (directory.Name,
                new[]
                {
                    fileHashed
                }
            )
        };

        var sut = CreateSut(directories);

        A.CallTo(() => inner.GetFileInfo(path))
            .Returns(fileNormal);

        A.CallTo(() => inner.GetFileInfo($"build/{fileHashed.Name}"))
            .Returns(fileHashed);

        var actual = sut.GetFileInfo(path);

        Assert.Equal(fileHashed, actual);
    }

    [Fact]
    public void Should_not_get_file_from_hashed_version_if_normal_file_exists()
    {
        var fileNormal = CreateFile("styles.css");
        var fileHashed = CreateFile("styles.42efefef.css");

        var directories = new[]
        {
            (string.Empty,
                new[]
                {
                    fileNormal
                }
            )
        };

        var sut = CreateSut(directories);

        A.CallTo(() => inner.GetFileInfo(fileNormal.Name))
            .Returns(fileNormal);

        A.CallTo(() => inner.GetFileInfo(fileHashed.Name))
            .Returns(fileHashed);

        var actual = sut.GetFileInfo(fileNormal.Name);

        Assert.Equal(fileNormal, actual);
    }

    [Fact]
    public void Should_not_get_file_from_hashed_version_if_normal_file_is_directory()
    {
        var fileNormal = CreateFile("styles.css", directory: true);
        var fileHashed = CreateFile("styles.42efefef.css");

        var directories = new[]
        {
            (string.Empty,
                new[]
                {
                    fileNormal
                }
            )
        };

        var sut = CreateSut(directories);

        A.CallTo(() => inner.GetFileInfo(fileNormal.Name))
            .Returns(fileNormal);

        A.CallTo(() => inner.GetFileInfo(fileHashed.Name))
            .Returns(fileHashed);

        var actual = sut.GetFileInfo(fileNormal.Name);

        Assert.Equal(fileNormal, actual);
    }

    [Fact]
    public void Should_not_get_file_from_hashed_version_if_not_mapped()
    {
        var fileNormal = CreateFile("styles.css");
        var fileHashed = CreateFile("styles.42efefef.css");

        var sut = CreateSut();

        A.CallTo(() => inner.GetFileInfo(fileNormal.Name))
            .Returns(fileNormal);

        A.CallTo(() => inner.GetFileInfo(fileHashed.Name))
            .Returns(fileHashed);

        var actual = sut.GetFileInfo(fileNormal.Name);

        Assert.Equal(fileNormal, actual);
    }

    [Fact]
    public void Should_forward_watch_call_to_inner()
    {
        var sut = CreateSut();

        sut.Watch("/");

        A.CallTo(() => inner.Watch("/"))
            .MustHaveHappened();
    }

    [Fact]
    public void Should_forward_directory_call_to_inner()
    {
        var sut = CreateSut();

        sut.GetDirectoryContents("/");

        A.CallTo(() => inner.GetDirectoryContents("/"))
            .MustHaveHappened();
    }

    private IgnoreHashFileProvider CreateSut(params (string Path, IFileInfo[] Files)[] directories)
    {
        foreach (var directory in directories)
        {
            A.CallTo(() => inner.GetDirectoryContents(directory.Path))
                .Returns(new DirectoryContents(directory.Files));
        }

        return new IgnoreHashFileProvider(inner);
    }

    private static IFileInfo CreateFile(string name, bool exists = true, bool directory = false)
    {
        return new File(name, exists, directory);
    }

    public record File(string Name, bool Exists = true, bool IsDirectory = false, long Length = 100) : IFileInfo
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
    {
        public DateTimeOffset LastModified => default;

        public string? PhysicalPath => default;

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }

    private sealed class DirectoryContents : List<IFileInfo>, IDirectoryContents
    {
        bool IDirectoryContents.Exists => true;

        public DirectoryContents(IEnumerable<IFileInfo> files)
            : base(files)
        {
        }
    }
}
